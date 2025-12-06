using System;

namespace ECommerce.Application.Common.Responses
{
    public interface IApiResult
    {
        bool Success { get; }
        string? Message { get; }
        int StatusCode { get; }
        DateTime Timestamp { get; }
    }
}