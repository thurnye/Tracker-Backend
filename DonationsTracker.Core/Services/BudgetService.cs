using System.Text;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;
using DonationsTracker.Core.Cache;

namespace DonationsTracker.Core.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<BudgetService> _logger;
        private readonly CacheInvalidationService _invalidation;

        private const string BudgetListPrefix = "budgets:list";
        private const string BudgetItemPrefix = "budgets:item:";

        public BudgetService(
            IBudgetRepository budgetRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<BudgetService> logger,
            CacheInvalidationService invalidation)
        {
            _budgetRepository = budgetRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<IEnumerable<Budget>> GetUserBudgetsAsync()
        {
            var userId = _userContext.GetUserId();
            string cacheKey = $"{BudgetListPrefix}:{userId}";

            try
            {
                var cached = _cache.GetString(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return JsonSerializer.Deserialize<IEnumerable<Budget>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed, falling back to DB for user {UserId}", userId);
            }

            var budgets = await _budgetRepository.GetBudgetsByUserAsync(userId);

            try
            {
                var json = JsonSerializer.Serialize(budgets);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                };
                await _cache.SetStringAsync(cacheKey, json, options);
                _logger.LogInformation("Cached budgets for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache budgets for user {UserId}", userId);
            }

            return budgets;
        }

        public async Task<Budget?> GetBudgetAsync(string id)
        {
            string cacheKey = $"{BudgetItemPrefix}{id}";

            try
            {
                var cached = _cache.GetString(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for budget {Id}", id);
                    return JsonSerializer.Deserialize<Budget>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for budget {Id}", id);
            }

            var budget = await _budgetRepository.GetBudgetByIdAsync(id);
            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found.");

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(budget),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
                );
                _logger.LogInformation("Cached budget {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache budget {Id}", id);
            }

            return budget;
        }

        public async Task<Budget> CreateUpdateBudgetAsync(Budget budget)
        {
            var userId = _userContext.GetUserId();
            budget.UserId = userId;

            Budget saved;
            if (!string.IsNullOrEmpty(budget.Id))
            {
                var existing = await _budgetRepository.GetBudgetByIdAsync(budget.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Budget not found.");

                if (existing.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s budget.");

                existing.BudgetAmount = budget.BudgetAmount;
                existing.CategoryId = budget.CategoryId;
                existing.Currency = budget.Currency;
                existing.Status = budget.Status;
                existing.SpendingType = budget.SpendingType;
                existing.StartDate = budget.StartDate;
                existing.EndDate = budget.EndDate;
                existing.Frequency = budget.Frequency;
                existing.Notes = budget.Notes;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsActive = true;

                saved = await _budgetRepository.UpdateBudgetAsync(existing);
            }
            else
            {
                budget.CreatedAt = DateTime.UtcNow;
                budget.IsActive = true;
                saved = await _budgetRepository.CreateBudgetAsync(budget);
            }

            // Invalidate cache
            _ = _invalidation.InvalidateByPrefixAsync(BudgetListPrefix);
            _ = _invalidation.InvalidateKeyAsync($"{BudgetItemPrefix}{saved.Id}");
            _logger.LogInformation("Cache invalidated after budget create/update for user {UserId}", userId);

            return saved;
        }

        public async Task<bool> DeleteBudgetAsync(string id)
        {
            var deleted = await _budgetRepository.DeleteBudgetAsync(id);
            if (deleted)
            {
                _ = _invalidation.InvalidateKeyAsync($"{BudgetItemPrefix}{id}");
                _ = _invalidation.InvalidateByPrefixAsync(BudgetListPrefix);
                _logger.LogInformation("Cache invalidated after delete of budget {Id}", id);
            }
            return deleted;
        }
    }
}
