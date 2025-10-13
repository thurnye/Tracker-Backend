using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Interfaces.Repositories;
using DonationsTracker.Core.Helpers;

namespace DonationsTracker.Core.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;
        private readonly IUserContextService _userContext;

        public WalletService(IWalletRepository walletRepository, IUserContextService userContext)
        {
            _walletRepository = walletRepository;
            _userContext = userContext;
        }

        public Task<IEnumerable<Wallet>> GetUserWalletsAsync()
        {
            var userId = _userContext.GetUserId();
            return _walletRepository.GetWalletsByUserAsync(userId);
        }

        public Task<Wallet?> GetWalletAsync(string id)
        {
            return _walletRepository.GetWalletByIdAsync(id);
        }

        public async Task<Wallet> CreateUpdateWalletAsync(Wallet wallet)
        {
            var userId = _userContext.GetUserId();
            wallet.UserId = userId;

            // ✅ Update if Id is present
            if (!string.IsNullOrEmpty(wallet.Id))
            {
                var existing = await _walletRepository.GetWalletByIdAsync(wallet.Id);
                if (existing == null)
                    throw new KeyNotFoundException("Wallet not found.");

                if (existing.UserId != userId)
                    throw new UnauthorizedAccessException("You cannot modify another user’s wallet.");

                existing.WalletName = wallet.WalletName;
                existing.WalletType = wallet.WalletType;
                existing.BankName = wallet.BankName;
                existing.AccountNumber = wallet.AccountNumber;
                existing.Currency = wallet.Currency;
                existing.Balance = wallet.Balance;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.IsActive = true;

                return await _walletRepository.UpdateWalletAsync(existing);
            }

            // ✅ Otherwise, create new
            wallet.CreatedAt = DateTime.UtcNow;
            wallet.IsActive = true;

            return await _walletRepository.CreateWalletAsync(wallet);
        }

        public async Task<bool> DeleteWalletAsync(string id)
        {
            return await _walletRepository.DeleteWalletAsync(id);
        }
    }
}
