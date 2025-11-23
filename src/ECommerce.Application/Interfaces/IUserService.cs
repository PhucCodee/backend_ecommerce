using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserProfileDto> GetProfileAsync(int userId);
    }
}