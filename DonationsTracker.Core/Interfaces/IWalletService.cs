using DonationsTracker.Core.Entity;

namespace DonationsTracker.Core.Interfaces
{
    public interface IWalletService
    {
        Task<IEnumerable<WalletDTO>> GetUserWalletsAsync();
        Task<WalletDTO?> GetWalletAsync(string id);
        Task<WalletDTO> CreateUpdateWalletAsync(Wallet wallet);
        Task<bool> DeleteWalletAsync(string id);
    }
}
