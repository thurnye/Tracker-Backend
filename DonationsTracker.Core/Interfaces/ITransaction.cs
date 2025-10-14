using DonationsTracker.Core.Entity;
using DonationsTracker.Core.RequestModel;

namespace DonationsTracker.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionDTO>> GetUserTransactionsAsync();
        Task<TransactionDTO?> GetTransactionAsync(string id);
        Task<TransactionDTO> CreateUpdateTransactionAsync(TransactionRequest transaction);
        Task<bool> DeleteTransactionAsync(string id);
    }
}
