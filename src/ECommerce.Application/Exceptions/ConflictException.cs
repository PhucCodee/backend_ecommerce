using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Exceptions
{
    public sealed class ConflictException(string message) : AppException(message, StatusCodes.Status409Conflict)
    {
    }
}