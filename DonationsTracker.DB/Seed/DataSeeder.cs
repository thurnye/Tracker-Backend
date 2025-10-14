using DonationsTracker.DB.Entities;
using Microsoft.AspNetCore.Identity;

namespace DonationsTracker.DB.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(DonationDbContext context, UserManager<ApplicationUser> userManager)
        {
            var user = await UserSeeder.SeedDefaultUserAsync(userManager);
            await CategorySeeder.SeedDefaultCategoriesAsync(context, user.Id);
        }
    }
}
