using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces.Repositories
{
    public interface IWalletRepository
    {
        Task<IEnumerable<Wallet>> GetWalletsByUserAsync(string userId);
        Task<Wallet?> GetWalletByIdAsync(string id);
        Task<Wallet> CreateWalletAsync(Wallet wallet);
        Task<Wallet?> UpdateWalletAsync(Wallet wallet);
        Task<bool> DeleteWalletAsync(string id);
    }
}
