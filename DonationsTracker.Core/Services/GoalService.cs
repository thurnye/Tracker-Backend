using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Cache;
using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.RequestModel;
using DonationsTracker.Core.Helpers;
using Microsoft.Extensions.Configuration;

namespace DonationsTracker.Core.Services
{
    public class GoalService : IGoalService
    {
        private readonly IGoalRepository _goalRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<GoalService> _logger;
        private readonly CacheInvalidationService _invalidation;
        private readonly TimeSpan _defaultTtl;

        private const string GoalListPrefix = "goals:list";
        private const string GoalItemPrefix = "goals:item:";

        public GoalService(
            IGoalRepository goalRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<GoalService> logger,
            CacheInvalidationService invalidation,
            IConfiguration config)
        {
            _goalRepository = goalRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<IEnumerable<GoalDTO>> GetUserGoalsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{GoalListPrefix}:{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for goals {UserId}", userId);
                    return JsonSerializer.Deserialize<IEnumerable<GoalDTO>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for goals {UserId}", userId);
            }

            var goals = await _goalRepository.GetGoalsByUserAsync(userId);
            var mapped = goals.Select(g => g.ToGoalDTO()).ToList();

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });

                await _invalidation.TrackKeyAsync(GoalListPrefix, cacheKey);
                _logger.LogInformation("Cached goals for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache goals {UserId}", userId);
            }

            return mapped;
        }

        public async Task<GoalDTO?> GetGoalAsync(string id)
        {
            var cacheKey = $"{GoalItemPrefix}{id}";
            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for goal {Id}", id);
                    return JsonSerializer.Deserialize<GoalDTO>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for goal {Id}", id);
            }

            var goal = await _goalRepository.GetGoalByIdAsync(id);
            if (goal == null) throw new KeyNotFoundException($"Goal {id} not found.");

            var mapped = goal.ToGoalDTO();

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });

                await _invalidation.TrackKeyAsync(GoalItemPrefix, cacheKey);
                _logger.LogInformation("Cached goal {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache goal {Id}", id);
            }

            return mapped;
        }

        public async Task<GoalDTO> CreateUpdateGoalAsync(GoalRequest goal)
        {
            var userId = _userContext.GetUserId();
            Goal saved;

            if (!string.IsNullOrEmpty(goal.Id))
            {
                var existing = await _goalRepository.GetGoalByIdAsync(goal.Id);
                if (existing == null) throw new KeyNotFoundException("Goal not found.");
                if (existing.UserId != userId) throw new UnauthorizedAccessException();

                existing.GoalName = goal.GoalName;
                existing.GoalDescription = goal.GoalDescription;
                existing.Priority = goal.Priority;
                existing.TargetValue = goal.TargetValue;
                existing.Deadline = goal.Deadline;
                existing.CategoryId = goal.CategoryId;
                existing.Progress = goal.Progress;
                existing.StartDate = goal.StartDate;
                existing.TargetMetric = goal.TargetMetric;
                existing.TargetDate = goal.TargetDate;
                existing.SuccessCriteria = goal.SuccessCriteria;
                existing.Actions = goal.Actions;
                existing.ResourcesNeeded = goal.ResourcesNeeded;
                existing.Obstacles = goal.Obstacles;
                existing.Milestones = goal.Milestones;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsActive = true;

                saved = await _goalRepository.UpdateGoalAsync(existing);
            }
            else
            {
                var newGoal = new Goal
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    GoalName = goal.GoalName,
                    GoalDescription = goal.GoalDescription,
                    Priority = goal.Priority,
                    TargetValue = goal.TargetValue,
                    Deadline = goal.Deadline,
                    CategoryId = goal.CategoryId,
                    Progress = goal.Progress,
                    CreatedAt = DateTime.UtcNow,
                    StartDate = goal.StartDate,
                    TargetMetric = goal.TargetMetric,
                    TargetDate = goal.TargetDate,
                    SuccessCriteria = goal.SuccessCriteria,
                    Actions = goal.Actions,
                    ResourcesNeeded = goal.ResourcesNeeded,
                    Obstacles = goal.Obstacles,
                    Milestones = goal.Milestones,
                    IsActive = true
                };
                saved = await _goalRepository.CreateGoalAsync(newGoal);
            }

            await _invalidation.InvalidateKeyAsync($"{GoalItemPrefix}{saved.Id}");
            await _invalidation.InvalidateByPrefixAsync(GoalListPrefix);

            _logger.LogInformation("Cache invalidated for goal {Id}", saved.Id);
            return saved.ToGoalDTO();
        }

        public async Task<bool> DeleteGoalAsync(string id)
        {
            var deleted = await _goalRepository.DeleteGoalAsync(id);
            if (deleted)
            {
                await _invalidation.InvalidateKeyAsync($"{GoalItemPrefix}{id}");
                await _invalidation.InvalidateByPrefixAsync(GoalListPrefix);
            }
            return deleted;
        }
    }
}
