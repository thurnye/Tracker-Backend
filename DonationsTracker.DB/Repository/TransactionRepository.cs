using DonationsTracker.Core.Entity;
using DonationsTracker.Core.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly DonationDbContext _context;

        public TransactionRepository(DonationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByUserAsync(string userId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .Where(t => t.UserId == userId && t.IsActive)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(string id)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Wallet)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction?> UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> DeleteTransactionAsync(string id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;

            transaction.IsActive = false;
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
