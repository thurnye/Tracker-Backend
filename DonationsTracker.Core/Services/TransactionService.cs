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
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TransactionService> _logger;
        private readonly CacheInvalidationService _invalidation;
        private readonly TimeSpan _defaultTtl;

        private const string TransactionListPrefix = "transactions:list";
        private const string TransactionItemPrefix = "transactions:item:";

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<TransactionService> logger,
            CacheInvalidationService invalidation,
            IConfiguration config)
        {
            _transactionRepository = transactionRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;


        }

        public async Task<(IEnumerable<TransactionDTO> Transactions, PaginationMeta Pagination)> GetUserTransactionsAsync(int page, int limit)
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{TransactionListPrefix}:{userId}:page:{page}:limit:{limit}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("✅ Cache hit for user transactions page {Page} of {UserId}", page, userId);
                    var cachedData = JsonSerializer.Deserialize<PaginatedCache<List<TransactionDTO>>>(cached);
                    return (cachedData!.Data, cachedData.Meta);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for user transactions {UserId}", userId);
            }

            var (transactions, totalCount) = await _transactionRepository.GetTransactionsByUserAsync(userId, page, limit);
            var mapped = transactions.Select(t => t.ToTransactionDTO()).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)limit);
            var pagination = new PaginationMeta { Page = page, Limit = limit, Total = totalCount, TotalPages = totalPages };

            try
            {
                var cacheData = new PaginatedCache<List<TransactionDTO>> { Data = mapped, Meta = pagination };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(cacheData),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl }
                );

                await _invalidation.TrackKeyAsync(TransactionListPrefix, cacheKey);
                _logger.LogInformation("💾 Cached user transactions page {Page} for {UserId}", page, userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache transactions page {Page} for {UserId}", page, userId);
            }

            return (mapped, pagination);
        }
        public async Task<TransactionDTO?> GetTransactionAsync(string id)
        {
            var cacheKey = $"{TransactionItemPrefix}{id}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("✅ Cache hit for transaction {Id}", id);
                    return JsonSerializer.Deserialize<TransactionDTO>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for transaction {Id}", id);
            }

            var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");

            var mapped = transaction.ToTransactionDTO();

            try
            {
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(mapped),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl });

                await _invalidation.TrackKeyAsync(TransactionItemPrefix, cacheKey);
                _logger.LogInformation("💾 Cached transaction {Id}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache transaction {Id}", id);
            }

            return mapped;
        }

        public async Task<TransactionDTO> CreateUpdateTransactionAsync(TransactionRequest transaction)
        {
            var userId = _userContext.GetUserId();
            Transaction saved;

            if (!string.IsNullOrEmpty(transaction.Id))
            {
                var existing = await _transactionRepository.GetTransactionByIdAsync(transaction.Id);
                if (existing == null) throw new KeyNotFoundException("Transaction not found.");
                if (existing.UserId != userId) throw new UnauthorizedAccessException();

                existing.TransactionAmount = transaction.TransactionAmount;
                existing.CategoryId = transaction.CategoryId;
                existing.TransactionType = transaction.TransactionType;
                existing.Description = transaction.Description;
                existing.TransactionDate = transaction.TransactionDate;
                existing.Method = transaction.Method;
                existing.WalletId = transaction.WalletId;
                existing.Status = transaction.Status;
                existing.MerchantName = transaction.MerchantName;
                existing.Location = transaction.Location;
                existing.Reference = transaction.Reference;
                existing.CurrencyCode = transaction.CurrencyCode;
                existing.TransactionFee = transaction.TransactionFee;
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;

                saved = await _transactionRepository.UpdateTransactionAsync(existing);
            }
            else
            {
                var newTransaction = new Transaction
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    WalletId = transaction.WalletId,
                    CategoryId = transaction.CategoryId,
                    TransactionDate = transaction.TransactionDate,
                    TransactionAmount = transaction.TransactionAmount,
                    TransactionType = transaction.TransactionType,
                    MerchantName = transaction.MerchantName,
                    Description = transaction.Description,
                    Status = transaction.Status,
                    Method = transaction.Method,
                    Location = transaction.Location,
                    Reference = transaction.Reference,
                    CurrencyCode = transaction.CurrencyCode,
                    TransactionFee = transaction.TransactionFee,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                };
                saved = await _transactionRepository.CreateTransactionAsync(newTransaction);
            }

            await _invalidation.InvalidateKeyAsync($"{TransactionItemPrefix}{saved.Id}");
            await _invalidation.InvalidateByPrefixAsync(TransactionListPrefix);
            _logger.LogInformation("🧹 Cache invalidated after transaction create/update {Id}", saved.Id);

            return saved.ToTransactionDTO();
        }

        public async Task<bool> DeleteTransactionAsync(string id)
        {
            var deleted = await _transactionRepository.DeleteTransactionAsync(id);
            if (deleted)
            {
                await _invalidation.InvalidateKeyAsync($"{TransactionItemPrefix}{id}");
                await _invalidation.InvalidateByPrefixAsync(TransactionListPrefix);
                _logger.LogInformation("🧹 Cache invalidated after deleting transaction {Id}", id);
            }
            return deleted;
        }

        public async Task<(IEnumerable<TransactionDTO> Transactions, PaginationMeta Pagination)>
    GetTransactionsByWalletIdAsync(string walletId, int page, int limit)
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{TransactionListPrefix}:{userId}:wallet:{walletId}:page:{page}:limit:{limit}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("✅ Cache hit for wallet {WalletId} page {Page}", walletId, page);
                    var cachedData = JsonSerializer.Deserialize<PaginatedCache<List<TransactionDTO>>>(cached);
                    return (cachedData!.Data, cachedData.Meta);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for wallet {WalletId}", walletId);
            }

            var (transactions, totalCount) = await _transactionRepository.GetTransactionsByWalletIdAsync(walletId, userId, page, limit);
            var mapped = transactions.Select(t => t.ToTransactionDTO()).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)limit);
            var pagination = new PaginationMeta { Page = page, Limit = limit, Total = totalCount, TotalPages = totalPages };

            try
            {
                var cacheData = new PaginatedCache<List<TransactionDTO>> { Data = mapped, Meta = pagination };
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(cacheData),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _defaultTtl }
                );

                await _invalidation.TrackKeyAsync(TransactionListPrefix, cacheKey);
                _logger.LogInformation("💾 Cached wallet {WalletId} page {Page}", walletId, page);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache wallet transactions {WalletId}", walletId);
            }

            return (mapped, pagination);
        }
    }
}
