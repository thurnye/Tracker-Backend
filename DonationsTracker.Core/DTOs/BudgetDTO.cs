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

        // 🔗 Lightweight nested DTOs
        public UserLiteDto? User { get; set; }
        public CategoryLiteDto? Category { get; set; }
    }

    public class UserLiteDto
    {
        public string Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class CategoryLiteDto
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
    }
}
