using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;
using DonationsTracker.Core.Cache;

namespace DonationsTracker.Core.Services
{
    public class GoalService : IGoalService
    {
        private readonly IGoalRepository _goalRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<GoalService> _logger;
        private readonly CacheInvalidationService _invalidation;

        private const string GoalListPrefix = "goals:list";
        private const string GoalItemPrefix = "goals:item:";

        public GoalService(
            IGoalRepository goalRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<GoalService> logger,
            CacheInvalidationService invalidation)
        {
            _goalRepository = goalRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<IEnumerable<Goal>> GetUserGoalsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{GoalListPrefix}:{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation(" Cache hit for goals {UserId}", userId);
                    return JsonSerializer.Deserialize<IEnumerable<Goal>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for goals {UserId}", userId);
            }

            var goals = await _goalRepository.GetGoalsByUserAsync(userId);

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(goals),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache goals {UserId}", userId);
            }

            return goals;
        }

        public async Task<Goal?> GetGoalAsync(string id)
        {
            var cacheKey = $"{GoalItemPrefix}{id}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation(" Cache hit for goal {Id}", id);
                    return JsonSerializer.Deserialize<Goal>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for goal {Id}", id);
            }

            var goal = await _goalRepository.GetGoalByIdAsync(id);
            if (goal == null)
                throw new KeyNotFoundException($"Goal with ID {id} not found.");

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(goal),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache goal {Id}", id);
            }

            return goal;
        }

        public async Task<Goal> CreateUpdateGoalAsync(Goal goal)
        {
            var userId = _userContext.GetUserId();
            goal.UserId = userId;
            Goal saved;

            if (!string.IsNullOrEmpty(goal.Id))
            {
                var existingGoal = await _goalRepository.GetGoalByIdAsync(goal.Id);
                if (existingGoal == null)
                    throw new KeyNotFoundException("Goal not found.");

                if (existingGoal.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s goal.");

                existingGoal.GoalName = goal.GoalName;
                existingGoal.GoalDescription = goal.GoalDescription;
                existingGoal.Priority = goal.Priority;
                existingGoal.TargetValue = goal.TargetValue;
                existingGoal.Deadline = goal.Deadline;
                existingGoal.CategoryId = goal.CategoryId;
                existingGoal.IsActive = true;
                existingGoal.UpdatedAt = DateTime.UtcNow;

                saved = await _goalRepository.UpdateGoalAsync(existingGoal);
            }
            else
            {
                goal.CreatedAt = DateTime.UtcNow;
                goal.IsActive = true;
                saved = await _goalRepository.CreateGoalAsync(goal);
            }

            _ = _invalidation.InvalidateKeyAsync($"{GoalItemPrefix}{saved.Id}");
            _ = _invalidation.InvalidateByPrefixAsync(GoalListPrefix);
            _logger.LogInformation("Cache invalidated for goal {Id}", saved.Id);

            return saved;
        }

        public async Task<bool> DeleteGoalAsync(string id)
        {
            var deleted = await _goalRepository.DeleteGoalAsync(id);
            if (deleted)
            {
                _ = _invalidation.InvalidateKeyAsync($"{GoalItemPrefix}{id}");
                _ = _invalidation.InvalidateByPrefixAsync(GoalListPrefix);
            }
            return deleted;
        }
    }
}
