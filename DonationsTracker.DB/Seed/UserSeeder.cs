using DonationsTracker.DB.Entities;
using Microsoft.AspNetCore.Identity;

namespace DonationsTracker.DB.Seed
{
    public static class UserSeeder
    {
        public static async Task<ApplicationUser> SeedDefaultUserAsync(UserManager<ApplicationUser> userManager)
        {
            var email = "test@test.com";
            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
                return existingUser;

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = email,
                Email = email,
                FirstName = "John",
                LastName = "Doe",
                Avatar = "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?ixlib=rb-4.1.0&auto=format&fit=crop&q=80&w=2070",
                Birthdate = DateTime.Parse("2025-10-13"),
                Gender = "Male",
                PhoneNumber = "+1234-456-6789",
                Address = "1234 Main Street",
                City = "Toronto",
                State = "Ontario",
                Country = "Canada",
                PostalCode = "a1b2c3",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, "strongPassword123!");
            if (!result.Succeeded)
                throw new Exception($"❌ Failed to seed user: {string.Join(", ", result.Errors.Select(e => e.Description))}");

            return user;
        }
    }
}
