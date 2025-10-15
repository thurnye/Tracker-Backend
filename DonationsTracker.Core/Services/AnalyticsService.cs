using System.Text.Json;
using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DonationsTracker.Core.Helpers;

namespace DonationsTracker.Core.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IAnalyticsRepository _repo;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly CacheInvalidationService _invalidation;
        private readonly TimeSpan _defaultTtl;

        private const string CachePrefix = "analytics:dashboard:";

        public AnalyticsService(
            IAnalyticsRepository repo,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<AnalyticsService> logger,
            CacheInvalidationService invalidation,
            IConfiguration config)
        {
            _repo = repo;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{CachePrefix}{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("✅ Cache hit for analytics {UserId}", userId);
                    return JsonSerializer.Deserialize<DashboardAnalyticsDTO>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for analytics {UserId}", userId);
            }

            var analytics = await _repo.GetDashboardAnalyticsAsync(userId);

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(analytics),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });

                await _invalidation.TrackKeyAsync(CachePrefix, cacheKey);
                _logger.LogInformation("💾 Cached analytics for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache analytics {UserId}", userId);
            }

            return analytics;
        }
    }
}
