using System;
using System.Collections.Generic;

namespace DonationsTracker.Api.Models
{
    public class ApiResponse<T>
    {
        public T? Data { get; set; }
        public ApiMeta Meta { get; set; } = new();
        public List<ApiError>? Errors { get; set; }
    }

    public class ApiMeta
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public PaginationMeta? Pagination { get; set; }
    }

    public class PaginationMeta
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public int TotalPages { get; set; }
    }

    public class ApiError
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Field { get; set; }
    }

    public static class ErrorCode
    {
        public const string VALIDATION_ERROR = "VALIDATION_ERROR";
        public const string AUTHENTICATION_ERROR = "AUTHENTICATION_ERROR";
        public const string AUTHORIZATION_ERROR = "AUTHORIZATION_ERROR";
        public const string NOT_FOUND = "NOT_FOUND";
        public const string CONFLICT_ERROR = "CONFLICT_ERROR";
        public const string RATE_LIMIT_ERROR = "RATE_LIMIT_ERROR";
        public const string INTERNAL_ERROR = "INTERNAL_ERROR";
        public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";
    }
    
}
