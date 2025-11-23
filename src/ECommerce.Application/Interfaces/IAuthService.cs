using ECommerce.Application.DTOs.auth;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string accessToken);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<bool> ConfirmEmailAsync(string token, string email);
    }
}