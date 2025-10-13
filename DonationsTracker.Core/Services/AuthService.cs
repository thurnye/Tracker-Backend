using DonationsTracker.Core.Helpers;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Models;
using DonationsTracker.DB;
using DonationsTracker.DB.Entities;
using DonationsTracker.DB.Repositories;

namespace DonationsTracker.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepo;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly JwtService _jwt;

        public AuthService(IAuthRepository authRepo, IRefreshTokenRepository refreshRepo, JwtService jwt)
        {
            _authRepo = authRepo;
            _refreshRepo = refreshRepo;
            _jwt = jwt;
        }

        public async Task<TokenPair> RegisterAsync(RegisterRequest model)
        {
            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Avatar = model.Avatar
            };

            var createdUser = await _authRepo.CreateUserAsync(user, model.Password);

            var accessToken = _jwt.GenerateAccessToken(createdUser);
            var refreshToken = _jwt.GenerateRefreshToken();

            await _refreshRepo.CreateAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = createdUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            var userResponse = new User
            {
                Id = createdUser.Id,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Email = createdUser.Email,
                Avatar = createdUser.Avatar
            };

            return new TokenPair
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userResponse
            };
        }

        public async Task<TokenPair> LoginAsync(LoginRequest model)
        {
            var user = await _authRepo.FindByEmailAsync(model.Email)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            var valid = await _authRepo.CheckPasswordAsync(user, model.Password);
            if (!valid)
                throw new UnauthorizedAccessException("Invalid credentials.");

            var accessToken = _jwt.GenerateAccessToken(user);
            var refreshToken = _jwt.GenerateRefreshToken();

            await _refreshRepo.CreateAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            var userResponse = new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Avatar = user.Avatar
            };

            return new TokenPair
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = userResponse
            };
        }

        public async Task<TokenPair> RefreshTokensAsync(string refreshToken)
        {
            var existing = await _refreshRepo.GetByTokenAsync(refreshToken);
            if (existing == null || existing.ExpiresAt < DateTime.UtcNow || existing.Revoked)
                throw new UnauthorizedAccessException("Invalid refresh token.");

            if (existing.User == null)
                throw new UnauthorizedAccessException("User not found for refresh token.");

            var newAccess = _jwt.GenerateAccessToken(existing.User);
            var newRefresh = _jwt.GenerateRefreshToken();

            await _refreshRepo.ReplaceAsync(existing, new RefreshToken
            {
                Token = newRefresh,
                UserId = existing.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            var userResponse = new User
            {
                Id = existing.User.Id,
                FirstName = existing.User.FirstName ?? string.Empty,
                LastName = existing.User.LastName ?? string.Empty,
                Email = existing.User.Email ?? string.Empty,
                Avatar = existing.User.Avatar ?? string.Empty
            };

            return new TokenPair
            {
                AccessToken = newAccess,
                RefreshToken = newRefresh,
                User = userResponse
            };
        }
    }
}
