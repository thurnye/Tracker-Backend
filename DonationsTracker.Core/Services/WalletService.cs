using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Cache;
using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.RequestModel;
using DonationsTracker.Core.Helpers;

namespace DonationsTracker.Core.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<WalletService> _logger;
        private readonly CacheInvalidationService _invalidation;
        private readonly TimeSpan _defaultTtl;

        private const string WalletListPrefix = "wallets:list";
        private const string WalletItemPrefix = "wallets:item:";

        public WalletService(
            IWalletRepository walletRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<WalletService> logger,
            CacheInvalidationService invalidation,
            IConfiguration config)
        {
            _walletRepository = walletRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<IEnumerable<WalletDTO>> GetUserWalletsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{WalletListPrefix}:{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for wallets {UserId}", userId);
                    return JsonSerializer.Deserialize<IEnumerable<WalletDTO>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for wallets {UserId}", userId);
            }

            var wallets = await _walletRepository.GetWalletsByUserAsync(userId);
            var mapped = wallets.Select(w => w.ToWalletDTO()).ToList();

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });
                await _invalidation.TrackKeyAsync(WalletListPrefix, cacheKey);
                _logger.LogInformation("Cached wallets for {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache wallets for {UserId}", userId);
            }

            return mapped;
        }

        public async Task<WalletDTO?> GetWalletAsync(string id)
        {
            var cacheKey = $"{WalletItemPrefix}{id}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for wallet {Id}", id);
                    return JsonSerializer.Deserialize<WalletDTO>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for wallet {Id}", id);
            }

            var wallet = await _walletRepository.GetWalletByIdAsync(id);
            if (wallet == null)
                throw new KeyNotFoundException($"Wallet with ID {id} not found.");

            var mapped = wallet.ToWalletDTO();

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });
                await _invalidation.TrackKeyAsync(WalletItemPrefix, cacheKey);
                _logger.LogInformation("Cached wallet {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache wallet {Id}", id);
            }

            return mapped;
        }

        public async Task<WalletDTO> CreateUpdateWalletAsync(WalletRequest wallet)
        {
            var userId = _userContext.GetUserId();
            Wallet saved;

            if (!string.IsNullOrEmpty(wallet.Id))
            {
                var existing = await _walletRepository.GetWalletByIdAsync(wallet.Id);
                if (existing == null) throw new KeyNotFoundException("Wallet not found.");
                if (existing.UserId != userId) throw new UnauthorizedAccessException();

                existing.WalletName = wallet.WalletName;
                existing.WalletType = wallet.WalletType;
                existing.BankName = wallet.BankName;
                existing.AccountNumber = wallet.AccountNumber;
                existing.Currency = wallet.Currency;
                existing.Balance = wallet.Balance;
                existing.CreditLimit = wallet.CreditLimit;
                existing.InterestRate = wallet.InterestRate;
                existing.LastTransactionDate = wallet.LastTransactionDate;
                existing.PaymentDueDate = wallet.PaymentDueDate;
                existing.CardType = wallet.CardType;
                existing.ExpiryDate = wallet.ExpiryDate;
                existing.CVV = wallet.CVV;
                existing.CategoryId = wallet.CategoryId;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsActive = true;

                saved = await _walletRepository.UpdateWalletAsync(existing);
            }
            else
            {
                var newWallet = new Wallet
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    WalletName = wallet.WalletName,
                    WalletType = wallet.WalletType,
                    BankName = wallet.BankName,
                    AccountNumber = wallet.AccountNumber,
                    Currency = wallet.Currency,
                    Balance = wallet.Balance,
                    CreditLimit = wallet.CreditLimit,
                    InterestRate = wallet.InterestRate,
                    LastTransactionDate = wallet.LastTransactionDate,
                    PaymentDueDate = wallet.PaymentDueDate,
                    CardType = wallet.CardType,
                    ExpiryDate = wallet.ExpiryDate,
                    CVV = wallet.CVV,
                    CategoryId = wallet.CategoryId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                };
                saved = await _walletRepository.CreateWalletAsync(newWallet);
            }

            await _invalidation.InvalidateKeyAsync($"{WalletItemPrefix}{saved.Id}");
            await _invalidation.InvalidateByPrefixAsync(WalletListPrefix);
            _logger.LogInformation("Cache invalidated after wallet create/update {Id}", saved.Id);

            return saved.ToWalletDTO();
        }

        public async Task<bool> DeleteWalletAsync(string id)
        {
            var deleted = await _walletRepository.DeleteWalletAsync(id);
            if (deleted)
            {
                await _invalidation.InvalidateKeyAsync($"{WalletItemPrefix}{id}");
                await _invalidation.InvalidateByPrefixAsync(WalletListPrefix);
                _logger.LogInformation("Cache invalidated after deleting wallet {Id}", id);
            }
            return deleted;
        }
    }
}
