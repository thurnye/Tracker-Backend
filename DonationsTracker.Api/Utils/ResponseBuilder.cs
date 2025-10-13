using System.Collections.Generic;
using DonationsTracker.Api.Models;

namespace DonationsTracker.Api.Helpers
{
    public static class ResponseBuilder
    {
        public static ApiResponse<T> Success<T>(T data)
        {
            return new ApiResponse<T>
            {
                Data = data,
                Errors = null
            };
        }

        public static ApiResponse<T> Error<T>(
            string message,
            string code = ErrorCode.INTERNAL_ERROR,
            string? field = null)
        {
            return new ApiResponse<T>
            {
                Data = default,
                Errors = new List<ApiError>
                {
                    new ApiError
                    {
                        Code = code,
                        Message = message,
                        Field = field
                    }
                }
            };
        }
    }
}
