using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces.Repositories
{
    public interface IGoalRepository
    {
        Task<IEnumerable<Goal>> GetGoalsByUserAsync(string userId);
        Task<Goal?> GetGoalByIdAsync(string id);
        Task<Goal> CreateGoalAsync(Goal goal);
        Task<Goal?> UpdateGoalAsync(Goal goal);
        Task<bool> DeleteGoalAsync(string id);
    }
}
