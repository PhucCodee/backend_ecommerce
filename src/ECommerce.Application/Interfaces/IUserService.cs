using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.user;

namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UserUpdateDto updateDto);
        Task<PagedResult<UserDto>> GetAllPagedAsync(PaginationParams paginationParams);
        Task<UserDto> GetByIdAsync(int userId);
        Task<UserDto> CreateAsync(UserCreateDto createDto);
        Task<UserDto> UpdateAsync(int userId, UserUpdateDto updateDto);
        Task<bool> DeleteAsync(int userId);
    }
}
