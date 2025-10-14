using DonationsTracker.Core.Entity;
using DonationsTracker.Core.RequestModel;

namespace DonationsTracker.Core.Interfaces
{
    public interface IWalletService
    {
        Task<IEnumerable<WalletDTO>> GetUserWalletsAsync();
        Task<WalletDTO?> GetWalletAsync(string id);
        Task<WalletDTO> CreateUpdateWalletAsync(WalletRequest wallet);
        Task<bool> DeleteWalletAsync(string id);
    }
}
