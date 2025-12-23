using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Exceptions
{
    public sealed class BadRequestException(string message) : AppException(message, StatusCodes.Status400BadRequest)
    {
    }
}