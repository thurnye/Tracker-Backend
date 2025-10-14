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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CategoryService> _logger;
        private readonly CacheInvalidationService _invalidation;

        private const string CategoryListPrefix = "categories:list";
        private const string CategoryItemPrefix = "categories:item:";

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<CategoryService> logger,
            CacheInvalidationService invalidation)
        {
            _categoryRepository = categoryRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<IEnumerable<Category>> GetUserCategoriesAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{CategoryListPrefix}:{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation(" Cache hit for categories {UserId}", userId);
                    return JsonSerializer.Deserialize<IEnumerable<Category>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for categories {UserId}", userId);
            }

            var categories = await _categoryRepository.GetCategoriesByUserAsync(userId);

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(categories),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache categories {UserId}", userId);
            }

            return categories;
        }

        public async Task<Category?> GetCategoryAsync(string id)
        {
            var cacheKey = $"{CategoryItemPrefix}{id}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation(" Cache hit for category {Id}", id);
                    return JsonSerializer.Deserialize<Category>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for category {Id}", id);
            }

            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found.");

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(category),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache category {Id}", id);
            }

            return category;
        }

        public async Task<Category> CreateUpdateCategoryAsync(Category category)
        {
            var userId = _userContext.GetUserId();
            category.UserId = userId;
            Category saved;

            if (!string.IsNullOrEmpty(category.Id))
            {
                var existing = await _categoryRepository.GetCategoryByIdAsync(category.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Category not found.");

                existing.Name = category.Name;
                existing.Icon = category.Icon;
                existing.Color = category.Color;
                existing.Type = category.Type;
                existing.UpdatedAt = DateTime.UtcNow;

                saved = await _categoryRepository.UpdateCategoryAsync(existing);
            }
            else
            {
                category.Id = Guid.NewGuid().ToString();
                category.CreatedAt = DateTime.UtcNow;
                saved = await _categoryRepository.CreateCategoryAsync(category);
            }

            _ = _invalidation.InvalidateByPrefixAsync(CategoryListPrefix);
            _ = _invalidation.InvalidateKeyAsync($"{CategoryItemPrefix}{saved.Id}");
            _logger.LogInformation(" Cache invalidated for category {Id}", saved.Id);

            return saved;
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var deleted = await _categoryRepository.DeleteCategoryAsync(id);
            if (deleted)
            {
                _ = _invalidation.InvalidateKeyAsync($"{CategoryItemPrefix}{id}");
                _ = _invalidation.InvalidateByPrefixAsync(CategoryListPrefix);
            }
            return deleted;
        }
    }
}
