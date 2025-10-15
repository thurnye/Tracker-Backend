using DonationsTracker.Api.Helpers;
using DonationsTracker.Api.Models;
using DonationsTracker.Core.Interfaces;
using DonationsTracker.Core.Models;
using DonationsTracker.Core.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DonationsTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly BotDetectionService _botDetection;

        public AuthController(IAuthService authService, BotDetectionService botDetection)
        {
            _authService = authService;
            _botDetection = botDetection;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (_botDetection.IsSuspiciousRequest(Request))
            {
                return BadRequest(new ApiResponse<object>
                {
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.VALIDATION_ERROR,
                            Message = "Suspicious or automated request detected. Please try again manually."
                        }
                    }
                });
            }

            // Register user and get access/refresh tokens
            var response = await _authService.RegisterAsync(model);
            var accessToken = response.AccessToken;
            var refreshToken = response.RefreshToken;

            // Store refresh token securely in HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // required for SameSite=None
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            Console.WriteLine($"[Auth] Refresh token cookie set for user");

            // Send access token via secure response header
            Response.Headers.Append("X-Access-Token", accessToken);
            Response.Headers.Append("Access-Control-Expose-Headers", "X-Access-Token");

            // Return user info only — no token data in JSON body
            return Ok(new ApiResponse<object> { Data = response.User });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var response = await _authService.LoginAsync(model);
            var accessToken = response.AccessToken;
            var refreshToken = response.RefreshToken;

            // Set refresh token as HttpOnly cookie
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // required for SameSite=None
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
            Console.WriteLine($"[Auth] Refresh token cookie set for user");

            // Return access token via response header
            Response.Headers.Append("X-Access-Token", accessToken);
            Response.Headers.Append("Access-Control-Expose-Headers", "X-Access-Token");

            return Ok(new ApiResponse<object> { Data = response.User });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            // Log all cookies for debugging
            var allCookies = string.Join(", ", Request.Cookies.Select(c => $"{c.Key}={c.Value.Substring(0, Math.Min(10, c.Value.Length))}..."));
            Console.WriteLine($"[Auth Refresh] All cookies: {allCookies}");

            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                Console.WriteLine("[Auth Refresh]  No refreshToken cookie found");
                return Unauthorized(new { message = "Missing refresh token" });
            }

            Console.WriteLine($"[Auth Refresh] Refresh token found in cookie");

            try
            {
                var tokens = await _authService.RefreshTokensAsync(refreshToken);
                Console.WriteLine($"[Auth Refresh] Tokens generated successfully");

                // Replace cookie with new refresh token
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // required for SameSite=None
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(7)
                };
                Response.Cookies.Append("refreshToken", tokens.RefreshToken, cookieOptions);
                Console.WriteLine($"[Auth Refresh] New refresh token cookie set");

                // Return access token via header
                Response.Headers.Append("X-Access-Token", tokens.AccessToken);
                Response.Headers.Append("Access-Control-Expose-Headers", "X-Access-Token");
                Console.WriteLine($"[Auth Refresh] Access token header set");

                // Return user data in the response body
                Console.WriteLine($"[Auth Refresh] Returning user data: {tokens.User.Email}");
                return Ok(new ApiResponse<object> { Data = tokens.User });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Auth Refresh]  Error: {ex.Message}");
                Console.WriteLine($"[Auth Refresh]  Stack trace: {ex.StackTrace}");
                return Unauthorized(new { message = "Invalid refresh token", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

            return Ok(new ApiResponse<object>
            {
                Data = new { Email = email }
            });
        }
    }
}
