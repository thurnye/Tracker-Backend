using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface IGoalService
    {
        Task<IEnumerable<GoalDTO>> GetUserGoalsAsync();
        Task<GoalDTO?> GetGoalAsync(string id);
        Task<GoalDTO> CreateUpdateGoalAsync(Goal goal);  
        Task<bool> DeleteGoalAsync(string id);
    }
}
