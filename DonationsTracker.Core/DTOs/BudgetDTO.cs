using DonationsTracker.Core.DTOs.Shared;

namespace DonationsTracker.Core.DTOs
{
    public class BudgetDTO
    {
        public string Id { get; set; }
        public string BudgetName { get; set; }
        public decimal BudgetAmount { get; set; }
        public string SpendingType { get; set; }
        public string? Currency { get; set; }
        public string? Status { get; set; }
        public DateTime Date { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Frequency { get; set; }
        public string? Notes { get; set; }
        public string? BudgetType { get; set; }
        public string? IncomeSource { get; set; }
        public decimal? AmountSpent { get; set; }
        public bool IsActive { get; set; }
        public string PaymentMethod { get; set; }

        
        public UserLiteDTO? User { get; set; }
        public CategoryLiteDTO? Category { get; set; }
    }
}
