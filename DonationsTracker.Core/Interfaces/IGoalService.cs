using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.RequestModel;

namespace DonationsTracker.Core.Interfaces
{
    public interface IGoalService
    {
        Task<IEnumerable<GoalDTO>> GetUserGoalsAsync();
        Task<GoalDTO?> GetGoalAsync(string id);
        Task<GoalDTO> CreateUpdateGoalAsync(GoalRequest goal);  
        Task<bool> DeleteGoalAsync(string id);
    }
}
