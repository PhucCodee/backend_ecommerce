using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Exceptions
{
    public sealed class ForbiddenException(string message) : AppException(message, StatusCodes.Status403Forbidden)
    {
    }
}