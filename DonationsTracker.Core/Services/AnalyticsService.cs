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

        private const string DashboardCachePrefix = "analytics:dashboard:";
        private const string FullAnalysisCachePrefix = "analytics:full:";

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

        // === DASHBOARD ANALYTICS ===
        public async Task<DashboardAnalyticsDTO> GetDashboardAnalyticsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{DashboardCachePrefix}{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation(" Cache hit for dashboard analytics {UserId}", userId);
                    return JsonSerializer.Deserialize<DashboardAnalyticsDTO>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for dashboard analytics {UserId}", userId);
            }

            var analytics = await _repo.GetDashboardAnalyticsAsync(userId);

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(analytics),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });

                await _invalidation.TrackKeyAsync(DashboardCachePrefix, cacheKey);
                _logger.LogInformation("Cached dashboard analytics for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache dashboard analytics {UserId}", userId);
            }

            return analytics;
        }

        // === FULL ANALYSIS ===
        public async Task<AnalyticsDTO> GetFullAnalysisAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{FullAnalysisCachePrefix}{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for full analysis {UserId}", userId);
                    return JsonSerializer.Deserialize<AnalyticsDTO>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for full analysis {UserId}", userId);
            }

            var analysis = await _repo.GetFullAnalysisAsync(userId);

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(analysis),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });

                await _invalidation.TrackKeyAsync(FullAnalysisCachePrefix, cacheKey);
                _logger.LogInformation("Cached full analysis for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache full analysis {UserId}", userId);
            }

            return analysis;
        }
    }
}
