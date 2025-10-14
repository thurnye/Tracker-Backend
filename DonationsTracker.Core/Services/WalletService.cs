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
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<WalletService> _logger;
        private readonly CacheInvalidationService _invalidation;

        private const string WalletListPrefix = "wallets:list";
        private const string WalletItemPrefix = "wallets:item:";

        public WalletService(
            IWalletRepository walletRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<WalletService> logger,
            CacheInvalidationService invalidation)
        {
            _walletRepository = walletRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<IEnumerable<Wallet>> GetUserWalletsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{WalletListPrefix}:{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for wallets of user {UserId}", userId);
                    return JsonSerializer.Deserialize<IEnumerable<Wallet>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for wallets of user {UserId}", userId);
            }

            var wallets = await _walletRepository.GetWalletsByUserAsync(userId);

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(wallets),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }
                );
                _logger.LogInformation("Cached wallets for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache wallets for user {UserId}", userId);
            }

            return wallets;
        }

        public async Task<Wallet?> GetWalletAsync(string id)
        {
            var cacheKey = $"{WalletItemPrefix}{id}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for wallet {Id}", id);
                    return JsonSerializer.Deserialize<Wallet>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for wallet {Id}", id);
            }

            var wallet = await _walletRepository.GetWalletByIdAsync(id);
            if (wallet == null)
                throw new KeyNotFoundException($"Wallet with ID {id} not found.");

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(wallet),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache wallet {Id}", id);
            }

            return wallet;
        }

        public async Task<Wallet> CreateUpdateWalletAsync(Wallet wallet)
        {
            var userId = _userContext.GetUserId();
            wallet.UserId = userId;
            Wallet saved;

            if (!string.IsNullOrEmpty(wallet.Id))
            {
                var existing = await _walletRepository.GetWalletByIdAsync(wallet.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Wallet not found.");

                if (existing.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s wallet.");

                existing.WalletName = wallet.WalletName;
                existing.WalletType = wallet.WalletType;
                existing.BankName = wallet.BankName;
                existing.AccountNumber = wallet.AccountNumber;
                existing.Currency = wallet.Currency;
                existing.Balance = wallet.Balance;
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;

                saved = await _walletRepository.UpdateWalletAsync(existing);
            }
            else
            {
                wallet.CreatedAt = DateTime.UtcNow;
                wallet.IsActive = true;
                saved = await _walletRepository.CreateWalletAsync(wallet);
            }

            _ = _invalidation.InvalidateByPrefixAsync(WalletListPrefix);
            _ = _invalidation.InvalidateKeyAsync($"{WalletItemPrefix}{saved.Id}");
            _logger.LogInformation("Cache invalidated after wallet create/update {Id}", saved.Id);

            return saved;
        }

        public async Task<bool> DeleteWalletAsync(string id)
        {
            var deleted = await _walletRepository.DeleteWalletAsync(id);
            if (deleted)
            {
                _ = _invalidation.InvalidateKeyAsync($"{WalletItemPrefix}{id}");
                _ = _invalidation.InvalidateByPrefixAsync(WalletListPrefix);
                _logger.LogInformation("🧹 Cache invalidated after deleting wallet {Id}", id);
            }
            return deleted;
        }
    }
}
