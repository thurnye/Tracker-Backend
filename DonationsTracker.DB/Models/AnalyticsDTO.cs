namespace DonationsTracker.Core.DTOs
{
    public class AnalyticsDTO
    {
        public List<AnalyticsMetricDTO> Metrics { get; set; } = new();
        public List<ExpenseCategoryDTO> ExpenseCategories { get; set; } = new();
        public List<MonthlyExpenseDTO> MonthlyExpenses { get; set; } = new();
        public List<TransactionSummaryDTO> Transactions { get; set; } = new();
    }

    public record AnalyticsMetricDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = "#10b981";
        public string Icon { get; set; } = "CircleDollarSign";
    }

    public record ExpenseCategoryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = "#3b82f6";
    }

    public record MonthlyExpenseDTO
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Saving { get; set; }
    }
}
