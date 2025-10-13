namespace DonationsTracker.Core.Models
{
    public class Wallet
    {
       public string Id { get; set; }
        public int UserId { get; set; }
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

        // Navigation
        public User User { get; set; } = null!;

    }
}
