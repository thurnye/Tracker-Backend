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
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserContextService _userContext;
        private readonly IDistributedCache _cache;
        private readonly ILogger<TransactionService> _logger;
        private readonly CacheInvalidationService _invalidation;

        private const string TransactionListPrefix = "transactions:list";
        private const string TransactionItemPrefix = "transactions:item:";

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserContextService userContext,
            IDistributedCache cache,
            ILogger<TransactionService> logger,
            CacheInvalidationService invalidation)
        {
            _transactionRepository = transactionRepository;
            _userContext = userContext;
            _cache = cache;
            _logger = logger;
            _invalidation = invalidation;
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync()
        {
            var userId = _userContext.GetUserId();
            var cacheKey = $"{TransactionListPrefix}:{userId}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for transactions of user {UserId}", userId);
                    return JsonSerializer.Deserialize<IEnumerable<Transaction>>(cached)!;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for transactions of user {UserId}", userId);
            }

            var transactions = await _transactionRepository.GetTransactionsByUserAsync(userId);

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(transactions),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }
                );
                _logger.LogInformation("Cached transactions for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache transactions for user {UserId}", userId);
            }

            return transactions;
        }

        public async Task<Transaction?> GetTransactionAsync(string id)
        {
            var cacheKey = $"{TransactionItemPrefix}{id}";

            try
            {
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit for transaction {Id}", id);
                    return JsonSerializer.Deserialize<Transaction>(cached);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis read failed for transaction {Id}", id);
            }

            var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");

            try
            {
                await _cache.SetStringAsync(
                    cacheKey,
                    JsonSerializer.Serialize(transaction),
                    new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
                );
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cache transaction {Id}", id);
            }

            return transaction;
        }

        public async Task<Transaction> CreateUpdateTransactionAsync(Transaction transaction)
        {
            var userId = _userContext.GetUserId();
            transaction.UserId = userId;
            Transaction saved;

            if (!string.IsNullOrEmpty(transaction.Id))
            {
                var existing = await _transactionRepository.GetTransactionByIdAsync(transaction.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Transaction not found.");

                if (existing.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s transaction.");

                existing.TransactionAmount = transaction.TransactionAmount;
                existing.CategoryId = transaction.CategoryId;
                existing.TransactionType = transaction.TransactionType;
                existing.Description = transaction.Description;
                existing.TransactionDate = transaction.TransactionDate;
                existing.Method = transaction.Method;
                existing.WalletId = transaction.WalletId;
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;

                saved = await _transactionRepository.UpdateTransactionAsync(existing);
            }
            else
            {
                transaction.CreatedAt = DateTime.UtcNow;
                transaction.IsActive = true;
                saved = await _transactionRepository.CreateTransactionAsync(transaction);
            }

            _ = _invalidation.InvalidateByPrefixAsync(TransactionListPrefix);
            _ = _invalidation.InvalidateKeyAsync($"{TransactionItemPrefix}{saved.Id}");
            _logger.LogInformation("Cache invalidated after transaction create/update {Id}", saved.Id);

            return saved;
        }

        public async Task<bool> DeleteTransactionAsync(string id)
        {
            var deleted = await _transactionRepository.DeleteTransactionAsync(id);
            if (deleted)
            {
                _ = _invalidation.InvalidateKeyAsync($"{TransactionItemPrefix}{id}");
                _ = _invalidation.InvalidateByPrefixAsync(TransactionListPrefix);
                _logger.LogInformation("🧹 Cache invalidated after deleting transaction {Id}", id);
            }
            return deleted;
        }
    }
}
