using DonationsTracker.Core.DTOs;
using DonationsTracker.Core.Entity;
using DonationsTracker.Core.RequestModel;

namespace DonationsTracker.Core.Interfaces
{
    public interface ITransactionService
    {
        Task<(IEnumerable<TransactionDTO> Transactions, PaginationMeta Pagination)> GetUserTransactionsAsync(int page, int limit);
        Task<(IEnumerable<TransactionDTO> Transactions, PaginationMeta Pagination)> GetTransactionsByWalletIdAsync(string walletId, int page, int limit);
        Task<TransactionDTO?> GetTransactionAsync(string id);
        Task<TransactionDTO> CreateUpdateTransactionAsync(TransactionRequest transaction);
        Task<bool> DeleteTransactionAsync(string id);
    }
}
