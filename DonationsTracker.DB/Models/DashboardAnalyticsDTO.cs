namespace DonationsTracker.Core.DTOs
{
    public class DashboardAnalyticsDTO
    {
        public List<BalanceCardDTO> BalanceCards { get; set; } = new();
        public List<MonthlyBudgetHealthDTO> MonthlyBudgetHealth { get; set; } = new();

        public List<ExpenseBreakdownDTO> ExpenseBreakdown { get; set; } = new();
        public List<MonthlySummaryDTO> MonthlyIncomeExpenses { get; set; } = new();
        public List<GoalSummaryDTO> SavingGoals { get; set; } = new();
        public List<BudgetSummaryDTO> BudgetGoals { get; set; } = new();
        public List<TransactionSummaryDTO> TransactionHistory { get; set; } = new();
        public List<PaymentHistoryDTO> PaymentsHistory { get; set; } = new();
        public List<WalletSpendingDTO> WalletSpending { get; set; } = new(); 
        public List<TransactionSummaryDTO> LatestTransactions { get; set; } = new();


    }

    public record BalanceCardDTO(string Id, string Title, decimal Amount, decimal Change, decimal ChangePercentage);

    public record MonthlyBudgetHealthDTO
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Budget { get; set; }
        public decimal Health { get; set; }
        public string Status { get; set; } = "Good";
    }

    public record ExpenseBreakdownDTO
    {
        public string Category { get; set; } = string.Empty;
        public double Percentage { get; set; }
        public decimal Amount { get; set; }
        public string Color { get; set; } = "#10b981";
        public string Icon { get; set; } = "credit-card";

    }

    public record MonthlySummaryDTO
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }

    public record GoalSummaryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Current { get; set; }
        public decimal Target { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = "#3b82f6";
        public string Icon { get; set; }
    }
    public record BudgetSummaryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Current { get; set; }
        public decimal Target { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = "#3b82f6";
        public string Icon { get; set; }
    }

    public record TransactionSummaryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "CAD";
        public string Icon { get; set; } = "credit-card";
        public string Color { get; set; } = "#10b981";
        public string MerchantName { get; set; }
    }

    public record PaymentHistoryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }

    public record WalletSpendingDTO
    {
        public string WalletId { get; set; } = string.Empty;
        public string WalletName { get; set; } = string.Empty;
        public string WalletType { get; set; } = string.Empty;
        public string Currency { get; set; } = "CAD";
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetBalance { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }

}
