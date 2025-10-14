using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DonationsTracker.Core.Cache
{
    /// <summary>
    /// Provides cache invalidation and key tracking for distributed caching.
    /// Uses Redis sets to safely track and delete keys by logical prefix.
    /// </summary>
    public class CacheInvalidationService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheInvalidationService> _logger;
        private readonly IConnectionMultiplexer _redis;

        public CacheInvalidationService(
            IDistributedCache cache,
            ILogger<CacheInvalidationService> logger,
            IConnectionMultiplexer redis)
        {
            _cache = cache;
            _logger = logger;
            _redis = redis;
        }

        /// <summary>
        /// Tracks a cache key under a logical prefix for later invalidation.
        /// For example, prefix = "goals:list" and key = "goals:list:user123"
        /// </summary>
        public async Task TrackKeyAsync(string prefix, string key)
        {
            try
            {
                var db = _redis.GetDatabase();
                await db.SetAddAsync($"{prefix}:keys", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to track cache key for prefix {Prefix}", prefix);
            }
        }

        /// <summary>
        /// Invalidates (deletes) all cache entries tracked under a prefix.
        /// </summary>
        public async Task InvalidateByPrefixAsync(string prefix)
        {
            try
            {
                var db = _redis.GetDatabase();
                var keySet = $"{prefix}:keys";
                var keys = (await db.SetMembersAsync(keySet)).Select(x => (string)x).ToList();

                if (keys.Any())
                {
                    var tasks = keys.Select(k => _cache.RemoveAsync(k));
                    await Task.WhenAll(tasks);
                    await db.KeyDeleteAsync(keySet);

                    _logger.LogInformation("🧹 Cleared {Count} cached items for prefix {Prefix}", keys.Count, prefix);
                }
                else
                {
                    _logger.LogInformation("ℹ️ No tracked keys found for prefix {Prefix}", prefix);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Cache invalidation failed for prefix {Prefix}", prefix);
            }
        }

        /// <summary>
        /// Invalidates a single cache entry immediately.
        /// </summary>
        public async Task InvalidateKeyAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
                _logger.LogInformation("🗑️ Cache key invalidated: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to invalidate cache key {Key}", key);
            }
        }

        /// <summary>
        /// Utility: Lists all tracked keys under a prefix (for debugging / logs).
        /// </summary>
        public async Task<IEnumerable<string>> ListTrackedKeysAsync(string prefix)
        {
            try
            {
                var db = _redis.GetDatabase();
                var keySet = $"{prefix}:keys";
                var keys = (await db.SetMembersAsync(keySet)).Select(x => (string)x);
                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Failed to list tracked keys for prefix {Prefix}", prefix);
                return Enumerable.Empty<string>();
            }
        }
    }
}
