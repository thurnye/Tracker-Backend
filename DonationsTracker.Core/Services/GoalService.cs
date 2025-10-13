using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;

namespace DonationsTracker.Core.Services
{
    public class GoalService : IGoalService
    {
        private readonly IGoalRepository _goalRepository;
        private readonly IUserContextService _userContext;

        public GoalService(IGoalRepository goalRepository, IUserContextService userContext)
        {
            _goalRepository = goalRepository;
            _userContext = userContext;
        }

        public Task<IEnumerable<Goal>> GetUserGoalsAsync()
        {
            var userId = _userContext.GetUserId();
            return _goalRepository.GetGoalsByUserAsync(userId);
        }

        public Task<Goal?> GetGoalAsync(string id)
        {
            return _goalRepository.GetGoalByIdAsync(id);
        }

        public async Task<Goal> CreateUpdateGoalAsync(Goal goal)
        {
            var userId = _userContext.GetUserId();
            goal.UserId = userId;

            // ✅ If ID exists → update
            if (!string.IsNullOrEmpty(goal.Id))
            {
                var existingGoal = await _goalRepository.GetGoalByIdAsync(goal.Id);
                if (existingGoal == null)
                    throw new KeyNotFoundException("Goal not found.");

                if (existingGoal.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s goal.");

                // Update fields
                existingGoal.GoalName = goal.GoalName;
                existingGoal.GoalDescription = goal.GoalDescription;
                existingGoal.Priority = goal.Priority;
                existingGoal.TargetValue = goal.TargetValue;
                existingGoal.Deadline = goal.Deadline;
                existingGoal.CategoryId = goal.CategoryId;
                existingGoal.IsActive = true;
                existingGoal.UpdatedAt = DateTime.UtcNow;

                return await _goalRepository.UpdateGoalAsync(existingGoal);
            }

            // Otherwise → create
            goal.CreatedAt = DateTime.UtcNow;
            goal.IsActive = true;
            return await _goalRepository.CreateGoalAsync(goal);
        }

        public async Task<bool> DeleteGoalAsync(string id)
        {
            return await _goalRepository.DeleteGoalAsync(id);
        }
    }
}
