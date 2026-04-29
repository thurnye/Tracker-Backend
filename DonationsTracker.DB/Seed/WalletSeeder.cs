using DonationsTracker.Core.Entity;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Seed
{
    public static class WalletSeeder
    {
        public static async Task<IEnumerable<Wallet>> SeedDefaultWalletsAsync(DonationDbContext context, string userId)
        {
            if (await context.Wallets.AnyAsync(w => w.UserId == userId))
                return Enumerable.Empty<Wallet>(); // skip if already seeded

            var now = DateTime.UtcNow;

            var wallets = new[]
            {
                new Wallet
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    WalletName = "Personal Checking",
                    WalletType = "Bank",
                    AccountNumber = "****1234",
                    BankName = "Royal Bank of Canada",
                    Currency = "CAD",
                    Balance = 5000m,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Wallet
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    WalletName = "Credit Card",
                    WalletType = "Credit",
                    AccountNumber = "****5678",
                    BankName = "TD Bank",
                    Currency = "CAD",
                    Balance = 0m,
                    CreditLimit = 10000m,
                    InterestRate = 19.99m,
                    CardType = "Visa",
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            };

            await context.Wallets.AddRangeAsync(wallets);
            await context.SaveChangesAsync();

            return wallets;
        }
    }
}
