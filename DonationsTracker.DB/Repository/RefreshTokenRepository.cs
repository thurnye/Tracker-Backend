using DonationsTracker.Core.Interfaces;
using DonationsTracker.DB.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationsTracker.DB.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DonationDbContext _context;

        public RefreshTokenRepository(DonationDbContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(t => t.User)  // Include the User entity
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(string userId)
        {
            return await _context.RefreshTokens
                .Where(t => t.UserId == userId && !t.Revoked)
                .ToListAsync();
        }

        public async Task<RefreshToken> CreateAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task InvalidateAsync(RefreshToken token)
        {
            token.Revoked = true;
            _context.RefreshTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task ReplaceAsync(RefreshToken oldToken, RefreshToken newToken)
        {
            oldToken.Revoked = true;
            _context.RefreshTokens.Update(oldToken);
            await _context.RefreshTokens.AddAsync(newToken);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
