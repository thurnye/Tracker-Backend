using System.Text;
using System.Text.Json;
using System.IO.Compression;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DonationsTracker.DB;
using DonationsTracker.DB.Repositories;
using DonationsTracker.Core.Cache;

namespace DonationsTracker.Core
{
    public class DonationServices : IDonationService
    {
        private readonly IDonationRepository _repo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<DonationServices> _logger;
        private readonly CacheInvalidationService _invalidation;
        private readonly TimeSpan _defaultTtl;

        private const string DonationListPrefix = "donations:list";
        private const string DonationItemPrefix = "donations:item:";

        public DonationServices(
            IDonationRepository repo,
            IDistributedCache cache,
            ILogger<DonationServices> logger,
            CacheInvalidationService invalidation,
            IConfiguration config)
        {
            _repo = repo;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public (List<Donation> Items, int TotalCount) GetDonations(int page, int limit)
        {
            string cacheKey = $"{DonationListPrefix}:{page}:{limit}";

            try
            {
                var cachedBytes = _cache.Get(cacheKey);
                if (cachedBytes != null)
                {
                    using var input = new MemoryStream(cachedBytes);
                    using var brotli = new BrotliStream(input, CompressionMode.Decompress);
                    using var reader = new StreamReader(brotli);
                    var json = reader.ReadToEnd();

                    var cachedResult = JsonSerializer.Deserialize<(List<Donation> Items, int TotalCount)>(json);
                    if (cachedResult.Items != null)
                    {
                        _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
                        return cachedResult;
                    }
                }

                _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed, falling back to DB.");
            }

            var (items, totalCount) = _repo.GetAll(page, limit);

            try
            {
                var result = (items, totalCount);
                var json = JsonSerializer.Serialize(result);

                using var output = new MemoryStream();
                using (var brotli = new BrotliStream(output, CompressionMode.Compress))
                {
                    var bytes = Encoding.UTF8.GetBytes(json);
                    brotli.Write(bytes, 0, bytes.Length);
                }

                var compressed = output.ToArray();

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _defaultTtl
                };

                _cache.Set(cacheKey, compressed, cacheOptions);
                _invalidation.TrackKeyAsync(DonationListPrefix, cacheKey).GetAwaiter().GetResult();
                _logger.LogInformation("Cached result for key: {CacheKey}", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache donations in Redis.");
            }

            return (items, totalCount);
        }

        public Donation GetDonationById(int id)
        {
            string cacheKey = $"{DonationItemPrefix}{id}";

            try
            {
                var cachedDonation = _cache.GetString(cacheKey);
                if (!string.IsNullOrEmpty(cachedDonation))
                {
                    _logger.LogInformation("Cache hit for donation ID: {Id}", id);
                    return JsonSerializer.Deserialize<Donation>(cachedDonation)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for donation ID {Id}", id);
            }

            var donation = _repo.GetById(id);
            if (donation == null)
                throw new KeyNotFoundException($"Donation with ID {id} not found.");

            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _defaultTtl
                };

                _cache.SetString(cacheKey, JsonSerializer.Serialize(donation), cacheOptions);
                _invalidation.TrackKeyAsync(DonationItemPrefix, cacheKey).GetAwaiter().GetResult();
                _logger.LogInformation("Cached donation ID {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache donation ID {Id}", id);
            }

            return donation;
        }

        public Donation CreateAndUpdate(Donation donation)
        {
            var saved = _repo.Save(donation);

            _invalidation.InvalidateKeyAsync($"{DonationItemPrefix}{saved.Id}").GetAwaiter().GetResult();
            _invalidation.InvalidateByPrefixAsync(DonationListPrefix).GetAwaiter().GetResult();

            _logger.LogInformation("Cache invalidated after Create/Update for donation ID {Id}", saved.Id);
            return saved;
        }

        public void DeleteDonation(int id)
        {
            _repo.Delete(id);

            _invalidation.InvalidateKeyAsync($"{DonationItemPrefix}{id}").GetAwaiter().GetResult();
            _invalidation.InvalidateByPrefixAsync(DonationListPrefix).GetAwaiter().GetResult();

            _logger.LogInformation("Cache invalidated after Delete for donation ID {Id}", id);
        }
    }
}
