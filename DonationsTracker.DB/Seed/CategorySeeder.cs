using DonationsTracker.Core.Entity;
using DonationsTracker.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Seed
{
    public static class CategorySeeder
    {
        public static async Task<IEnumerable<Category>> SeedDefaultCategoriesAsync(DonationDbContext context, string userId)
        {
            if (await context.Categories.AnyAsync(c => c.UserId == userId))
                return Enumerable.Empty<Category>(); // skip if already seeded

            var now = DateTime.UtcNow;

            var incomeCategories = new[]
            {
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Income", Name = "Salary", Icon = "Wallet", Color = "#10b981", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Income", Name = "Business", Icon = "BriefcaseBusiness", Color = "#3b82f6", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Income", Name = "Grants", Icon = "Banknote", Color = "#f59e0b", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Income", Name = "Gifts", Icon = "Gift", Color = "#8b5cf6", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Income", Name = "Refund", Icon = "RotateCcw", Color = "#ec4899", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Income", Name = "Loan", Icon = "TrendingUp", Color = "#06b6d4", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Income", Name = "Other", Icon = "EllipsisVertical", Color = "#64748b", CreatedAt = now, IsActive = true },
            // Expense categories
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Beauty", Icon = "Sparkles", Color = "#06b6d4", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Bills & Fees", Icon = "Receipt", Color = "#3b82f6", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Car", Icon = "Car", Color = "#f59e0b", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Education", Icon = "BookOpen", Color = "#8b5cf6", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Entertainment", Icon = "Tv", Color = "#ec4899", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Family", Icon = "Users", Color = "#10b981", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Food & Drink", Icon = "Utensils", Color = "#ef4444", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Gifts", Icon = "Gift", Color = "#f59e0b", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Insurance", Icon = "ShieldCheck", Color = "#3b82f6", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Home", Icon = "Home", Color = "#8b5cf6", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Shopping", Icon = "ShoppingBag", Color = "#ec4899", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Sports", Icon = "Dumbbell", Color = "#06b6d4", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Market", Icon = "Store", Color = "#10b981", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Travel", Icon = "Plane", Color = "#f59e0b", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Personal", Icon = "User", Color = "#64748b", CreatedAt = now, IsActive = true },
                new Category { Id = Guid.NewGuid().ToString(), UserId = userId, Type = "Expense", Name = "Gym", Icon = "Dumbbell", Color = "#ef4444", CreatedAt = now, IsActive = true },
            };

            await context.Categories.AddRangeAsync(incomeCategories);
            await context.SaveChangesAsync();

            return incomeCategories;
        }
    }
}
