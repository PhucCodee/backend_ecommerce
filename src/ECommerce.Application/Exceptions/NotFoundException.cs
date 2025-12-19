using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Exceptions
{
    public sealed class NotFoundException(string message) : AppException(message, StatusCodes.Status404NotFound)
    {
    }
}