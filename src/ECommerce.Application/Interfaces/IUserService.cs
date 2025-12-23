using ECommerce.Application.DTOs.user;
using ECommerce.Application.Common.Pagination;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<IEnumerable<UserProfileDto>> GetAllAsync();
        Task<PagedResult<UserProfileDto>> GetAllPagedAsync(PaginationParams paginationParams);
        Task<UserProfileDto> GetByIdAsync(int userId);
        Task<UserProfileDto> CreateAsync(UserCreateDto createDto);
        Task<UserProfileDto> UpdateAsync(int userId, UserUpdateDto updateDto);
        Task<UserProfileDto> UpdateProfileAsync(int userId, UserUpdateDto updateDto);
        Task<bool> DeleteAsync(int userId);
    }
}