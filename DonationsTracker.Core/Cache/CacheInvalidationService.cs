using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace DonationsTracker.Core.Cache
{
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
        /// Removes all cache entries matching a prefix pattern.
        /// Works with Redis directly to scan and delete keys.
        /// </summary>
        public async Task InvalidateByPrefixAsync(string prefix)
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: $"{prefix}*").ToArray();

                foreach (var key in keys)
                    await _cache.RemoveAsync(key);

                _logger.LogInformation("🧹 Cache invalidated for prefix: {Prefix} (Count: {Count})", prefix, keys.Length);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cache invalidation failed for prefix: {Prefix}", prefix);
            }
        }

        /// <summary>
        /// Removes a single cache key.
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
                _logger.LogWarning(ex, "Failed to invalidate cache key {Key}", key);
            }
        }
    }
}
