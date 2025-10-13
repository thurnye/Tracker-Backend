using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces.Repositories
{
    public interface ITransactionRepository
    {
        Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(string userId);
        Task<Transaction> GetTransactionByIdAsync(string id);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(string id);
    }
}
