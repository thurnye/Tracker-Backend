
using DonationsTracker.DB;

namespace DonationsTracker.Core.Entity
{
    public class Budget
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string? CategoryId { get; set; } 
        public string SpendingType { get; set; } = null!;  // e.g. "Monthly", "Weekly"
        public decimal BudgetAmount { get; set; }
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
        public Boolean? IsActive { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Category? Category { get; set; }

    }
}
