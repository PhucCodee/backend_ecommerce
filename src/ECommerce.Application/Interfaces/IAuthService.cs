using ECommerce.Application.DTOs.auth;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
        Task<AuthOperationResultDto> LogoutAsync(string accessToken);
        Task<AuthOperationResultDto> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<AuthOperationResultDto> ResetPasswordAsync(string email);
        Task<AuthOperationResultDto> ConfirmEmailAsync(string token, string email);
    }
}