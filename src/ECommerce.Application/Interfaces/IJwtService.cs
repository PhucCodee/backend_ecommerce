using System.Security.Claims;

namespace ECommerce.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(int userId, string email, string role);
        string GenerateAccessToken(int userId, string email, string[] roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? ValidateToken(string token);
    }
}