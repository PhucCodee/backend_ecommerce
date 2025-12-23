using System.Collections.Generic;

namespace ECommerce.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(int userId, string email, IEnumerable<string> roles);
        string GenerateRefreshToken();
        int? ValidateAccessToken(string token);
        int? ValidateRefreshToken(string token);
    }
}