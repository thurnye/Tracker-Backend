using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Entity;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Repositories
{
    public class BudgetRepository : IBudgetRepository 
    {
        private readonly DonationDbContext _context;

        public BudgetRepository(DonationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Budget>> GetBudgetsByUserAsync(string userId)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                 .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();
        }

        public async Task<Budget?> GetBudgetByIdAsync(string id)
        {
            return await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<Budget> CreateBudgetAsync(Budget budget)
        {
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
            return budget;
        }

        public async Task<Budget?> UpdateBudgetAsync(Budget budget)
        {
            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();
            return budget;
        }

        public async Task<bool> DeleteBudgetAsync(string id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return false;

            budget.IsActive = false; 
            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();

            return true;

        }
    }
}
