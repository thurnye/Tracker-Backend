using DonationsTracker.Core.Models;

namespace DonationsTracker.Core.Interfaces
{
    public interface IAuthService
    {
        Task<TokenPair> RegisterAsync(RegisterRequest model);
        Task<TokenPair> LoginAsync(LoginRequest model);
        Task<TokenPair> RefreshTokensAsync(string refreshToken);
    }
}
