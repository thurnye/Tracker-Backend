using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;
using DonationsTracker.Core.Cache;
using DonationsTracker.Core.DTOs;

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

        public async Task<IEnumerable<BudgetDTO>> GetUserBudgetsAsync()
        {
            var userId = _userContext.GetUserId();
            string cacheKey = $"{BudgetListPrefix}:{userId}";

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
                _logger.LogWarning(ex, "Redis read failed, falling back to DB for user {UserId}", userId);
            }

            var budgets = await _budgetRepository.GetBudgetsByUserAsync(userId);
            var mapped = budgets.Select(MapToDto).ToList();

            try
            {
                var json = JsonSerializer.Serialize(mapped);
                await _cache.SetStringAsync(cacheKey, json, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
                _logger.LogInformation("Cached budgets for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache budgets for user {UserId}", userId);
            }

            return mapped;
        }

        public async Task<BudgetDTO?> GetBudgetAsync(string id)
        {
            string cacheKey = $"{BudgetItemPrefix}{id}";

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

            var mapped = MapToDto(budget);

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                    }
                );
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

            // Invalidate cache
            _ = _invalidation.InvalidateByPrefixAsync(BudgetListPrefix);
            _ = _invalidation.InvalidateKeyAsync($"{BudgetItemPrefix}{saved.Id}");
            _logger.LogInformation("Cache invalidated after budget create/update for user {UserId}", userId);

            return MapToDto(saved);
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


        private static BudgetDTO MapToDto(Budget b)
        {
            return new BudgetDTO
            {
                Id = b.Id,
                BudgetName = b.BudgetName,
                BudgetAmount = b.BudgetAmount,
                SpendingType = b.SpendingType,
                Currency = b.Currency,
                Status = b.Status,
                Date = b.Date,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                Frequency = b.Frequency,
                Notes = b.Notes,
                BudgetType = b.BudgetType,
                IncomeSource = b.IncomeSource,
                AmountSpent = b.AmountSpent,
                IsActive = b.IsActive,

                // Include simplified user and category
                User = b.User != null
                    ? new UserLiteDto
                    {
                        Id = b.User.Id,
                        FirstName = b.User.FirstName,
                        LastName = b.User.LastName
                    }
                    : null,

                Category = b.Category != null
                    ? new CategoryLiteDto
                    {
                        Id = b.Category.Id,
                        Type = b.Category.Type,
                        Name = b.Category.Name,
                        Icon = b.Category.Icon,
                        Color = b.Category.Color
                    }
                    : null
            };
        }

    }
}
