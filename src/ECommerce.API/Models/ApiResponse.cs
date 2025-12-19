using System;
using System.Collections.Generic;

namespace ECommerce.Application.Common.Responses
{
    public sealed class ApiResponse<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? Message { get; init; }
        public List<string>? Errors { get; init; }
        public Dictionary<string, string[]>? ValidationErrors { get; init; }
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;

        private ApiResponse() { }

        public static ApiResponse<T> Ok(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message
            };
        }

        public static ApiResponse<T> Fail(
            string message,
            List<string>? errors = null,
            Dictionary<string, string[]>? validationErrors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                ValidationErrors = validationErrors
            };
        }
    }
}
