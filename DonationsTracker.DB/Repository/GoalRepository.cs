using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Repositories
{
    public class GoalRepository : IGoalRepository
    {
        private readonly DonationDbContext _context;

        public GoalRepository(DonationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Goal>> GetGoalsByUserAsync(string userId)
        {
            return await _context.Goals
                .Include(g => g.Category)
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();
        }

        public async Task<Goal?> GetGoalByIdAsync(string id)
        {
            return await _context.Goals
                .Include(g => g.Category)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<Goal> CreateGoalAsync(Goal goal)
        {
            _context.Goals.Add(goal);
            await _context.SaveChangesAsync();
            return goal;
        }

        public async Task<Goal?> UpdateGoalAsync(Goal goal)
        {
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();
            return goal;
        }

        public async Task<bool> DeleteGoalAsync(string id)
        {
            var goal = await _context.Goals.FindAsync(id);
            if (goal == null) return false;

            goal.IsActive = false; // Soft delete
            _context.Goals.Update(goal);
            await _context.SaveChangesAsync();

            return true;
        }

    }
}
