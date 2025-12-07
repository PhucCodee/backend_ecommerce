using System.Collections.Generic;

namespace ECommerce.Application.Common.Responses
{
    public interface IApiError : IApiResult
    {
        List<string> Errors { get; }
        Dictionary<string, string[]>? ValidationErrors { get; }
    }
}