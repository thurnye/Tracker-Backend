using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync();
        Task<Transaction?> GetTransactionAsync(string id);
        Task<Transaction> CreateUpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(string id);
    }
}
