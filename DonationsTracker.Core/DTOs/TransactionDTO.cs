
using DonationsTracker.Core.DTOs.Shared;
using DonationsTracker.DB;

namespace DonationsTracker.Core.Entity
{
    public class TransactionDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string? WalletId { get; set; }
        public string? CategoryId { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionType { get; set; } = null!;  // e.g. "Deposit", "Transfer", "Expense"
        public string? MerchantName { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }       // e.g. "Completed", "Pending", "Declined"
        public string? Method { get; set; }       // e.g. "ATM", "Card", "Online"
        public string? Location { get; set; }
        public string? Reference { get; set; }
        public string? CurrencyCode { get; set; }
        public decimal? TransactionFee { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public UserLiteDTO? User { get; set; }
        public CategoryLiteDTO? Category { get; set; }

    }
}
