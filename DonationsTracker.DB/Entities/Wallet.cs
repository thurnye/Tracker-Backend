
using DonationsTracker.DB;

namespace DonationsTracker.Core.Entity
{
    public class Wallet
    {
       public string Id { get; set; }
        public string UserId { get; set; }
        public string? CategoryId { get; set; } 
        public string WalletName { get; set; } = null!;  // e.g. "Personal Wallet", "Business Account"
        public string WalletType { get; set; } = null!;  // e.g. "Bank", "Credit", "Digital"
        public string AccountNumber { get; set; } = null!;
        public string BankName { get; set; } = null!;
        public string Currency { get; set; } = "USD";
        public decimal? Balance { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? InterestRate { get; set; }
        public DateTime? LastTransactionDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }

        // Optional card metadata
        public string? CardType { get; set; }     // e.g. "Visa", "Mastercard"
        public DateTime? ExpiryDate { get; set; }
        public int? CVV { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public Category? Category { get; set; }

    }
}
