using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.DTOs.Shared;
using DonationsTracker.Core.Entity;
using DonationsTracker.DB;

namespace DonationsTracker.Core.Helpers
{
    public static class DTOMappingExtensions
    {
        // Already existing:
        public static UserLiteDTO ToLiteDTO(this ApplicationUser user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        public static CategoryLiteDTO ToLiteDTO(this Category category) => new()
        {
            Id = category.Id,
            Type = category.Type,
            Name = category.Name,
            Icon = category.Icon,
            Color = category.Color
        };

        public static BudgetDTO ToBudgetDTO(this Budget b) => new()
        {
            Id = b.Id,
            BudgetName = b.BudgetName,
            BudgetAmount = b.BudgetAmount,
            SpendingType = b.SpendingType,
            Currency = b.Currency,
            Status = b.Status,
            Date = b.Date,
            StartDate = b.StartDate,
            EndDate = b.EndDate,
            Frequency = b.Frequency,
            Notes = b.Notes,
            BudgetType = b.BudgetType,
            IncomeSource = b.IncomeSource,
            AmountSpent = b.AmountSpent,
            IsActive = b.IsActive,
            PaymentMethod = b.PaymentMethod,
            User = b.User?.ToLiteDTO(),
            Category = b.Category?.ToLiteDTO()
        };

        public static GoalDTO ToGoalDTO(this Goal g) => new()
        {
            Id = g.Id,
            UserId = g.UserId,
            CategoryId = g.CategoryId,
            GoalName = g.GoalName,
            GoalDescription = g.GoalDescription,
            Priority = g.Priority,
            Progress = g.Progress,
            Deadline = g.Deadline,
            StartDate = g.StartDate,
            TargetMetric = g.TargetMetric,
            TargetValue = g.TargetValue,
            TargetDate = g.TargetDate,
            SuccessCriteria = g.SuccessCriteria,
            Actions = g.Actions,
            ResourcesNeeded = g.ResourcesNeeded,
            Obstacles = g.Obstacles,
            Milestones = g.Milestones,
            IsActive = g.IsActive,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt,
            User = g.User?.ToLiteDTO(),
            Category = g.Category?.ToLiteDTO()
        };

        // NEW: Transaction mapping
        public static TransactionDTO ToTransactionDTO(this Transaction t) => new()
        {
            Id = t.Id,
            UserId = t.UserId,
            WalletId = t.WalletId,
            CategoryId = t.CategoryId,
            TransactionDate = t.TransactionDate,
            TransactionAmount = t.TransactionAmount,
            TransactionType = t.TransactionType,
            MerchantName = t.MerchantName,
            Description = t.Description,
            Status = t.Status,
            Method = t.Method,
            Location = t.Location,
            Reference = t.Reference,
            CurrencyCode = t.CurrencyCode,
            TransactionFee = t.TransactionFee,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            User = t.User?.ToLiteDTO(),
            Category = t.Category?.ToLiteDTO()
        };

        // NEW: Wallet mapping
        public static WalletDTO ToWalletDTO(this Wallet w) => new()
        {
            Id = w.Id,
            UserId = w.UserId,
            CategoryId = w.CategoryId,
            WalletName = w.WalletName,
            WalletType = w.WalletType,
            AccountNumber = w.AccountNumber,
            BankName = w.BankName,
            Currency = w.Currency,
            Balance = w.Balance,
            CreditLimit = w.CreditLimit,
            InterestRate = w.InterestRate,
            LastTransactionDate = w.LastTransactionDate,
            PaymentDueDate = w.PaymentDueDate,
            CardType = w.CardType,
            ExpiryDate = w.ExpiryDate,
            CVV = w.CVV,
            IsActive = w.IsActive,
            CreatedAt = w.CreatedAt,
            UpdatedAt = w.UpdatedAt,
            User = w.User?.ToLiteDTO(),
            Category = w.Category?.ToLiteDTO()
        };
    }
}
