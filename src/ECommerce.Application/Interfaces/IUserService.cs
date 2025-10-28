using System;
using System.Threading.Tasks;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(UserDto userDto);
        Task<UserDto> LoginAsync(string email, string password);
        Task<UserDto> GetUserByIdAsync(Guid userId);
        Task<UserDto> UpdateUserAsync(UserDto userDto);
        Task<bool> DeleteUserAsync(Guid userId);
    }
}