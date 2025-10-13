using DonationsTracker.Core.Models;
using DonationsTracker.Core.Security;
using FluentValidation;
using Microsoft.AspNetCore.Http; 
using Microsoft.Extensions.Logging;

namespace DonationsTracker.Core.Validators
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly BotDetectionService _botService;
        private readonly ILogger<RegisterRequestValidator> _logger;

        public RegisterRequestValidator(
            IHttpContextAccessor httpContextAccessor,
            BotDetectionService botService,
            ILogger<RegisterRequestValidator> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _botService = botService;
            _logger = logger;

            // ✅ Input validation rules
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

            // ✅ Bot detection
            RuleFor(x => x)
                .Must(NotBeBotRequest)
                .WithMessage("Suspicious or automated request detected. Please try again manually.");
        }

        private bool NotBeBotRequest(RegisterRequest request)
        {
            try
            {
                var httpRequest = _httpContextAccessor.HttpContext?.Request;
                if (httpRequest == null) return true;

                var isBot = _botService.IsSuspiciousRequest(httpRequest);
                if (isBot)
                {
                    _logger.LogWarning("🚫 Bot-like activity detected for email: {Email}", request.Email);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bot detection failed.");
                return true; // fallback: don't block legit users if detection fails
            }
        }
    }
}
