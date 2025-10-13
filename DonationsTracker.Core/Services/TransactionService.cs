using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;

namespace DonationsTracker.Core.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserContextService _userContext;

        public TransactionService(ITransactionRepository transactionRepository, IUserContextService userContext)
        {
            _transactionRepository = transactionRepository;
            _userContext = userContext;
        }

        public Task<IEnumerable<Transaction>> GetUserTransactionsAsync()
        {
            var userId = _userContext.GetUserId();
            return _transactionRepository.GetTransactionsByUserAsync(userId);
        }

        public Task<Transaction?> GetTransactionAsync(string id)
        {
            return _transactionRepository.GetTransactionByIdAsync(id);
        }

        public async Task<Transaction> CreateUpdateTransactionAsync(Transaction transaction)
        {
            var userId = _userContext.GetUserId();
            transaction.UserId = userId;

            // ✅ If ID exists → update
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
                existing.Description = transaction.Description;
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;

                return await _transactionRepository.UpdateTransactionAsync(existing);
            }

            // ✅ Otherwise → create
            transaction.CreatedAt = DateTime.UtcNow;
            transaction.IsActive = true;
            return await _transactionRepository.CreateTransactionAsync(transaction);
        }

        public async Task<bool> DeleteTransactionAsync(string id)
        {
            return await _transactionRepository.DeleteTransactionAsync(id);
        }
    }
}
