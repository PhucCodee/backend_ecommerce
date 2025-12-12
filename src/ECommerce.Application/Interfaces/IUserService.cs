using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<IEnumerable<UserProfileDto>> GetAllAsync();
        Task<UserProfileDto?> GetByIdAsync(int userId);
        Task<UserProfileDto> CreateAsync(UserCreateDto createDto);
        Task<UserProfileDto?> UpdateAsync(int userId, UserUpdateDto updateDto);
        Task<bool> DeleteAsync(int userId);
    }
}