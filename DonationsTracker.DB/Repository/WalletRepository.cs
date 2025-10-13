using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly DonationDbContext _context;

        public WalletRepository(DonationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Wallet>> GetWalletsByUserAsync(string userId)
        {
            return await _context.Wallets
                .Include(w => w.Transactions)
                .Where(w => w.UserId == userId && w.IsActive)
                .ToListAsync();
        }

        public async Task<Wallet?> GetWalletByIdAsync(string id)
        {
            return await _context.Wallets
                .Include(w => w.Transactions)
                .FirstOrDefaultAsync(w => w.Id == id && w.IsActive);
        }

        public async Task<Wallet> CreateWalletAsync(Wallet wallet)
        {
            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<Wallet?> UpdateWalletAsync(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public async Task<bool> DeleteWalletAsync(string id)
        {
            var wallet = await _context.Wallets.FindAsync(id);
            if (wallet == null) return false;

            wallet.IsActive = false;
            _context.Wallets.Update(wallet);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
