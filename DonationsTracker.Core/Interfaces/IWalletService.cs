using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface IWalletService
    {
        Task<IEnumerable<Wallet>> GetUserWalletsAsync();
        Task<Wallet?> GetWalletAsync(string id);
        Task<Wallet> CreateUpdateWalletAsync(Wallet wallet);
        Task<bool> DeleteWalletAsync(string id);
    }
}
