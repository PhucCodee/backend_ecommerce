using System;
using System.Collections.Generic;

namespace ECommerce.Application.Common.Responses
{
    public class ApiResponse<T> : IApiResponse<T>, IApiError
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string> Errors { get; set; } = [];
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Success responses
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Operation completed successfully",
                StatusCode = 200
            };
        }

        public static ApiResponse<T> SuccessResponse(T data, string message, int statusCode)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = statusCode
            };
        }

        // Failure responses
        public static ApiResponse<T> Failure(string message, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> Failure(List<string> errors, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Errors = errors,
                StatusCode = statusCode,
                Message = "Validation failed"
            };
        }

        public static ApiResponse<T> Failure(string message, List<string> errors, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }

        public static ApiResponse<T> ValidationFailure(string message, Dictionary<string, string[]> errors, int statusCode = 400)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                ValidationErrors = errors,
                StatusCode = statusCode
            };
        }
    }
}