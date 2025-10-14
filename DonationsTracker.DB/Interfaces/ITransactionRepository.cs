using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetTransactionsByUserAsync(string userId, int page, int limit);
        Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetTransactionsByWalletIdAsync(string walletId, string userId, int page, int limit);
        Task<Transaction?> GetTransactionByIdAsync(string id);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(string id);
    }
}
