using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Exceptions
{
    public sealed class NotFoundException(string message) : AppException(message, StatusCodes.Status404NotFound)
    {
    }
}