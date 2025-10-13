namespace DonationsTracker.Core.Models
{
    public class Budget
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public string SpendingType { get; set; } = null!;  // e.g. "Monthly", "Weekly"
        public decimal BudgetAmount { get; set; }
        public string Category { get; set; } = null!;
        public DateTime Date { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Frequency { get; set; }
        public string? Notes { get; set; }
        public string? BudgetType { get; set; }    // e.g. "Personal", "Business"
        public string? IncomeSource { get; set; }
        public string? Currency { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public decimal? AmountSpent { get; set; }

        // Navigation
        public User User { get; set; } = null!;

    }
}
