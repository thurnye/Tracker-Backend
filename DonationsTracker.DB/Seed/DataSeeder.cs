using DonationsTracker.DB.Entities;
using Microsoft.AspNetCore.Identity;

namespace DonationsTracker.DB.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(DonationDbContext context, UserManager<ApplicationUser> userManager)
        {
            var user = await UserSeeder.SeedDefaultUserAsync(userManager);
            var categories = (await CategorySeeder.SeedDefaultCategoriesAsync(context, user.Id)).ToList();
            var wallets = (await WalletSeeder.SeedDefaultWalletsAsync(context, user.Id)).ToList();
            // Seed transactions 
            await TransactionSeeder.SeedTransactionsAsync(context, user.Id, categories, wallets);
        }
    }
}
