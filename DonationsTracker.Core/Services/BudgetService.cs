using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;
using DonationsTracker.Core.Cache;
using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.RequestModel;
using Microsoft.Extensions.Configuration;

namespace DonationsTracker.Core.Services
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<BudgetService> _logger;
        private readonly CacheInvalidationService _invalidation;
        private readonly TimeSpan _defaultTtl;

        private const string BudgetListPrefix = "budgets:list";
        private const string BudgetItemPrefix = "budgets:item:";

        public BudgetService(
            IBudgetRepository budgetRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<BudgetService> logger,
            CacheInvalidationService invalidation,
            IConfiguration config)
        {
            _budgetRepository = budgetRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;

        }

        public async Task<IEnumerable<BudgetDTO>> GetUserBudgetsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{BudgetListPrefix}:{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return JsonSerializer.Deserialize<IEnumerable<BudgetDTO>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for user {UserId}", userId);
            }

            var budgets = await _budgetRepository.GetBudgetsByUserAsync(userId);
            var mapped = budgets.Select(b => b.ToBudgetDTO()).ToList();

            try
            {
                var json = JsonSerializer.Serialize(mapped);
                await _cache.SetStringAsync(cacheKey, json,
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });

                await _invalidation.TrackKeyAsync(BudgetListPrefix, cacheKey);
                _logger.LogInformation("Cached budgets for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache budgets for {UserId}", userId);
            }

            return mapped;
        }

        public async Task<BudgetDTO?> GetBudgetAsync(string id)
        {
            var cacheKey = $"{BudgetItemPrefix}{id}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for budget {Id}", id);
                    return JsonSerializer.Deserialize<BudgetDTO>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for budget {Id}", id);
            }

            var budget = await _budgetRepository.GetBudgetByIdAsync(id);
            if (budget == null)
                throw new KeyNotFoundException($"Budget with ID {id} not found.");

            var mapped = budget.ToBudgetDTO();

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl }
                );
                await _invalidation.TrackKeyAsync(BudgetItemPrefix, cacheKey);
                _logger.LogInformation("Cached budget {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache budget {Id}", id);
            }

            return mapped;
        }

        public async Task<BudgetDTO> CreateUpdateBudgetAsync(BudgetRequest budget)
        {
            var userId = _userContext.GetUserId();
            Budget saved;

            if (!string.IsNullOrEmpty(budget.Id))
            {
                var existing = await _budgetRepository.GetBudgetByIdAsync(budget.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Budget not found.");

                if (existing.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s budget.");

                existing.BudgetName = budget.BudgetName;
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
                var newBudget = new Budget
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    CategoryId = budget.CategoryId,
                    SpendingType = budget.SpendingType,
                    BudgetAmount = budget.BudgetAmount,
                    BudgetName = budget.BudgetName,
                    Date = budget.Date,
                    PaymentMethod = budget.PaymentMethod,
                    Frequency = budget.Frequency,
                    Notes = budget.Notes,
                    BudgetType = budget.BudgetType,
                    IncomeSource = budget.IncomeSource,
                    Currency = budget.Currency,
                    StartDate = budget.StartDate,
                    EndDate = budget.EndDate,
                    Status = budget.Status,
                    AmountSpent = budget.AmountSpent,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                saved = await _budgetRepository.CreateBudgetAsync(newBudget);
            }

            // Await invalidations to clear immediately
            await _invalidation.InvalidateKeyAsync($"{BudgetItemPrefix}{saved.Id}");
            await _invalidation.InvalidateByPrefixAsync(BudgetListPrefix);
            _logger.LogInformation("Cache invalidated after budget create/update for {UserId}", userId);

            return saved.ToBudgetDTO();
        }

        public async Task<bool> DeleteBudgetAsync(string id)
        {
            var deleted = await _budgetRepository.DeleteBudgetAsync(id);
            if (deleted)
            {
                await _invalidation.InvalidateKeyAsync($"{BudgetItemPrefix}{id}");
                await _invalidation.InvalidateByPrefixAsync(BudgetListPrefix);
                _logger.LogInformation("Cache invalidated after deleting budget {Id}", id);
            }
            return deleted;
        }
    }
}
