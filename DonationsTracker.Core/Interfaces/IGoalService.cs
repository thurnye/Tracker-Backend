using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface IGoalService
    {
        Task<IEnumerable<Goal>> GetUserGoalsAsync();
        Task<Goal?> GetGoalAsync(string id);
        Task<Goal> CreateUpdateGoalAsync(Goal goal);  
        Task<bool> DeleteGoalAsync(string id);
    }
}
