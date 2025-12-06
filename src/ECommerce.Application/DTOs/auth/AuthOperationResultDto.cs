using System;

namespace ECommerce.Application.DTOs.auth
{
    public class AuthOperationResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static AuthOperationResultDto SuccessResult(string message)
        {
            return new AuthOperationResultDto
            {
                Success = true,
                Message = message
            };
        }

        public static AuthOperationResultDto FailureResult(string message)
        {
            return new AuthOperationResultDto
            {
                Success = false,
                Message = message
            };
        }
    }
}