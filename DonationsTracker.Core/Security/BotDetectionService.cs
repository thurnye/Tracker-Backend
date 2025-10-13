using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DonationsTracker.Core.Security
{
    public class BotDetectionService
    {
        private readonly ILogger<BotDetectionService> _logger;

        public BotDetectionService(ILogger<BotDetectionService> logger)
        {
            _logger = logger;
        }

        public bool IsSuspiciousRequest(HttpRequest request)
        {
            var userAgent = request.Headers["User-Agent"].ToString();
            var referer = request.Headers["Referer"].ToString();

            if (string.IsNullOrEmpty(userAgent))
                return true;

            if (userAgent.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
                userAgent.Contains("crawler", StringComparison.OrdinalIgnoreCase) ||
                userAgent.Contains("spider", StringComparison.OrdinalIgnoreCase))
                return true;

            if (referer.Contains("clickfarm") || referer.Contains("spam"))
                return true;

            _logger.LogInformation("Bot check passed for {UserAgent}", userAgent);
            return false;
        }
    }
}
