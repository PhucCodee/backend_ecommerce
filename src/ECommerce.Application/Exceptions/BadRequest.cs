using Microsoft.AspNetCore.Http;

namespace ECommerce.Application.Common.Exceptions
{
    public sealed class BadRequestException(string message) : AppException(message, StatusCodes.Status400BadRequest)
    {
    }
}