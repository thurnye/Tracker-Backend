using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync();
        Task<TransactionDTO?> GetTransactionAsync(string id);
        Task<TransactionDTO> CreateUpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(string id);
    }
}
