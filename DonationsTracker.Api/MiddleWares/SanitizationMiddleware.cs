using System.Text;
using System.Text.Json;
using DonationsTracker.Core.Security;
using DonationsTracker.Api.Models;

namespace DonationsTracker.Api.Middlewares.Security
{
    public class SanitizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SanitizationMiddleware> _logger;

        public SanitizationMiddleware(RequestDelegate next, ILogger<SanitizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Sanitize query parameters
                if (context.Request.Query.Count > 0)
                {
                    var sanitizedQuery = new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>();
                    foreach (var kv in context.Request.Query)
                    {
                        var cleanValue = InputSanitizer.Sanitize(kv.Value.ToString());
                        sanitizedQuery[kv.Key] = cleanValue;
                    }
                    context.Request.QueryString = QueryString.Create(sanitizedQuery);
                }

                // Sanitize JSON request body
                if (context.Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
                {
                    context.Request.EnableBuffering();

                    using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true);
                    var body = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        try
                        {
                            var sanitizedBody = SanitizeJson(body);
                            var bytes = Encoding.UTF8.GetBytes(sanitizedBody);
                            context.Request.Body = new MemoryStream(bytes);
                            context.Request.ContentLength = bytes.Length;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to sanitize JSON body.");
                        }
                    }
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error during sanitization middleware.");

                var response = new ApiResponse<object>
                {
                    Data = null,
                    Errors = new List<ApiError>
                    {
                        new ApiError
                        {
                            Code = ErrorCode.INTERNAL_ERROR,
                            Message = "An error occurred while sanitizing the request."
                        }
                    }
                };

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }

        private string SanitizeJson(string json)
        {
            using var document = JsonDocument.Parse(json);
            var sanitized = SanitizeElement(document.RootElement);
            return JsonSerializer.Serialize(sanitized);
        }

        private object SanitizeElement(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.Object => element.EnumerateObject()
                    .ToDictionary(p => p.Name, p => SanitizeElement(p.Value)),
                JsonValueKind.Array => element.EnumerateArray()
                    .Select(SanitizeElement).ToList(),
                JsonValueKind.String => InputSanitizer.Sanitize(element.GetString() ?? ""),
                JsonValueKind.Number => element.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => null!
            };
        }
    }
}
