using DonationsTracker.DB.Entities;
using DonationsTracker.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using DonationsTracker.Core.DTOs;

namespace DonationsTracker.DB.Repositories
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly DonationDbContext _context;

        public AnalyticsRepository(DonationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync(string userId)
        {
            var transactions = await _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();

            var goals = await _context.Goals
                .Include(g => g.Category)
                .Where(g => g.UserId == userId && g.IsActive)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId && b.IsActive)
                .ToListAsync();

            if (!transactions.Any() && !goals.Any() && !budgets.Any())
                return new DashboardAnalyticsDTO();

            var totalIncome = transactions
                .Where(t => t.TransactionType.ToLower() == "credit")
                .Sum(t => t.TransactionAmount);

            var totalExpenses = transactions
                .Where(t => t.TransactionType.ToLower() != "credit")
                .Sum(t => t.TransactionAmount);

            var totalBalance = totalIncome - totalExpenses;

            var now = DateTime.UtcNow;
            var last60 = transactions.Where(t => t.TransactionDate >= now.AddDays(-60)).ToList();
            var recent30 = last60.Where(t => t.TransactionDate >= now.AddDays(-30)).ToList();
            var prev30 = last60.Where(t => t.TransactionDate < now.AddDays(-30)).ToList();

            decimal recentExp = recent30
                .Where(t => t.TransactionType.ToLower() != "credit")
                .Sum(t => t.TransactionAmount);

            decimal prevExp = prev30
                .Where(t => t.TransactionType.ToLower() != "credit")
                .Sum(t => t.TransactionAmount);

            decimal change = recentExp - prevExp;
            decimal changePct = prevExp == 0 ? 0 : (change / prevExp) * 100;

            // Expense Breakdown (with category details)
            var expenseBreakdown = transactions
                .Where(t => t.TransactionType.ToLower() != "credit")
                .GroupBy(t => t.Category)
                .Select(g => new ExpenseBreakdownDTO
                {
                    Category = g.Key?.Name ?? "Uncategorized",
                    Amount = g.Sum(x => x.TransactionAmount),
                    Percentage = Math.Round((double)(g.Sum(x => x.TransactionAmount) / totalExpenses * 100), 2),
                    Color = g.Key?.Color ?? "#f59e0b"
                })
                .ToList();

            // Monthly summaries (Income vs Expense)
            var monthly = transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new MonthlySummaryDTO
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    Income = g.Where(t => t.TransactionType.ToLower() == "credit").Sum(t => t.TransactionAmount),
                    Expense = g.Where(t => t.TransactionType.ToLower() != "credit").Sum(t => t.TransactionAmount)
                })
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMM", null).Month)
                .ToList();

            // Total monthly income summary (aggregated separately)
            var monthlyIncomeOnly = transactions
                .Where(t => t.TransactionType.ToLower() == "credit")
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g => new MonthlySummaryDTO
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM"),
                    Income = g.Sum(t => t.TransactionAmount),
                    Expense = 0
                })
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMM", null).Month)
                .ToList();

            // Budget Summary (basic budgets)
            var budgetSummary = budgets.Select(b => new BudgetSummaryDTO
            {
                Id = b.Id,
                Name = b.BudgetName,
                Current = b.AmountSpent ?? 0,
                Target = b.BudgetAmount,
                Percentage = b.BudgetAmount > 0 ? Math.Round((double)((b.AmountSpent ?? 0) / b.BudgetAmount * 100), 2) : 0,
                Color = b.Category.Color,
                Icon = b.Category.Icon
            }).ToList();

            // Saving Goals summary
            var savingGoals = goals.Select(g => new GoalSummaryDTO
            {
                Id = g.Id,
                Name = g.GoalName,
                Current = (decimal?)g.Progress ?? 0,
                Target = g.TargetValue ?? 0,
                Percentage = g.TargetValue > 0 ? Math.Round((double)((g.Progress ?? 0) / g.TargetValue.Value * 100), 2) : 0,
                Color = g.Category.Color,
                Icon = g.Category.Icon
            }).ToList();

            // Recurring transactions (top 10 expensive recurring)
            var past90Days = transactions
                .Where(t => t.TransactionDate >= now.AddDays(-90) && t.TransactionType.ToLower() != "credit")
                .ToList();

            var recurringMerchants = past90Days
                .GroupBy(t => t.MerchantName)
                .Where(g => g.Count() >= 3)
                .Where(g =>
                {
                    var ordered = g.OrderBy(t => t.TransactionDate).ToList();
                    var intervals = ordered.Skip(1).Select((t, i) =>
                        (t.TransactionDate - ordered[i].TransactionDate).TotalDays).ToList();

                    if (!intervals.Any()) return false;

                    double avg = intervals.Average();
                    double deviation = intervals.Average(d => Math.Abs(d - avg));
                    bool regular = deviation <= 3;

                    var avgAmt = g.Average(t => (double)t.TransactionAmount);
                    bool consistentAmt = g.All(t => Math.Abs((double)t.TransactionAmount - avgAmt) / avgAmt <= 0.1);

                    return regular && consistentAmt;
                })
                .Select(g => g.Key)
                .ToList();

            var recurringTransactions = past90Days
                .Where(t => recurringMerchants.Contains(t.MerchantName))
                .OrderByDescending(t => t.TransactionAmount)
                .Take(10)
                .Select(t => new TransactionSummaryDTO
                {
                    Id = t.Id,
                    Category = t.Category?.Name ?? "General",
                    Date = t.TransactionDate.ToString("yyyy-MM-dd"),
                    Description = t.Description ?? t.MerchantName ?? "Recurring Payment",
                    MerchantName = t.MerchantName,
                    Amount = t.TransactionAmount,
                    Currency = t.CurrencyCode ?? "CAD",
                    Icon = t.Category.Icon,
                    Color = t.Category?.Color ?? "#ef4444"
                })
                .ToList();


            // === Top 10 Most Expensive Payments (Past 30 Days) ===
            var past30Days = DateTime.UtcNow.AddDays(-30);

            var paymentsHistory = await _context.Transactions
                .Where(t =>
                    t.Status.ToLower() == "completed" &&
                    t.TransactionDate >= past30Days &&
                    t.IsActive)
                .Include(t => t.Category) // include category info
                .OrderByDescending(t => t.TransactionAmount)
                .Take(10)
                .Select(t => new PaymentHistoryDTO
                {
                    Id = t.Id,
                    Name = t.MerchantName ?? "Unknown",
                    Date = t.TransactionDate.ToString("yyyy-MM-dd"),
                    Amount = t.TransactionAmount,
                    Currency = t.CurrencyCode ?? "CAD",
                    Status = "paid",
                    Category = t.Category != null ? t.Category.Name : "General",
                    Icon = t.Category != null ? t.Category.Icon : "credit-card",
                    Color = t.Category != null ? t.Category.Color : "#10b981"
                })
                .ToListAsync();





            // === Monthly Budget Health ===
            var monthlyBudgetHealth = transactions
                .GroupBy(t => new { t.TransactionDate.Year, t.TransactionDate.Month })
                .Select(g =>
                {
                    var monthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM");

                    // Income for this month
                    var income = g.Where(t => t.TransactionType.ToLower() == "credit").Sum(t => t.TransactionAmount);

                    // Budgets active during this month
                    var monthBudgets = budgets
                        .Where(b =>
                            (b.StartDate == null || b.StartDate.Value.Month <= g.Key.Month) &&
                            (b.EndDate == null || b.EndDate.Value.Month >= g.Key.Month))
                        .Sum(b => b.BudgetAmount);

                    // Compute Health (% remaining income after budgeting)
                    decimal health = 0;
                    string status = "Good";

                    if (income > 0)
                    {
                        var ratio = (monthBudgets / income) * 100;
                        health = Math.Round(100 - ratio, 2);
                        status = ratio > 100 ? "Overspent" : "Good";
                    }
                    else
                    {
                        health = 0;
                        status = "No Income";
                    }

                    return new MonthlyBudgetHealthDTO
                    {
                        Month = monthName,
                        Income = income,
                        Budget = monthBudgets,
                        Health = health,
                        Status = status
                    };
                })
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMMM", null).Month)
                .ToList();


            return new DashboardAnalyticsDTO
            {
                BalanceCards = new()
                {
                    new("1", "Total Balance", totalBalance, 10.5m, 2.5m),
                    new("2", "Total Period Change", change, change, changePct),
                    new("3", "Total Period Expenses", totalExpenses, change, changePct),
                    new("4", "Total Period Income", totalIncome, 12.3m, 5.2m)
                },
                MonthlyBudgetHealth = monthlyBudgetHealth,
                ExpenseBreakdown = expenseBreakdown,
                MonthlyIncomeExpenses = monthly,
                SavingGoals = savingGoals,
                BudgetGoals = budgetSummary,
                TransactionHistory = recurringTransactions,
                PaymentsHistory = paymentsHistory
            };
        }
    }
}
