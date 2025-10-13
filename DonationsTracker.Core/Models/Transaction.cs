namespace DonationsTracker.Core.Models
{
    public class Transaction
    {
        public string Id { get; set; }
        public int UserId { get; set; }
        public int? WalletId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionType { get; set; } = null!;  // e.g. "Deposit", "Transfer", "Expense"
        public string? MerchantName { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }     // e.g. "Food", "Transport"
        public string? Status { get; set; }       // e.g. "Completed", "Pending", "Declined"
        public string? Method { get; set; }       // e.g. "ATM", "Card", "Online"
        public string? Location { get; set; }
        public string? Reference { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? TransactionFee { get; set; }

        // Navigation
        public User User { get; set; } = null!;

    }
}
