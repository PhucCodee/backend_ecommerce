using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Exceptions
{
    public sealed class UnauthorizedException(string message) : AppException(message, StatusCodes.Status401Unauthorized)
    {
    }
}