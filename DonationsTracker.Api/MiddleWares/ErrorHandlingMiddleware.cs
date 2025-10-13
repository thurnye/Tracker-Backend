using System.Net;
using System.Text.Json;
using DonationsTracker.Api.Helpers;
using DonationsTracker.Api.Models;

namespace DonationsTracker.Api.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Not Found Error");
                await WriteError(context, HttpStatusCode.NotFound, ex.Message, ErrorCode.NOT_FOUND);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized");
                await WriteError(context, HttpStatusCode.Unauthorized, ex.Message, ErrorCode.AUTHORIZATION_ERROR);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception");
                await WriteError(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.", ErrorCode.INTERNAL_ERROR);
            }
        }

        private static async Task WriteError(HttpContext context, HttpStatusCode status, string message, string code)
        {
            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";

            var response = ResponseBuilder.Error<object>(message, code);
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
