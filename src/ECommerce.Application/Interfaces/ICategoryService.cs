using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.category;

namespace ECommerce.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryDto> GetByIdAsync(int categoryId);
        Task<CategoryDto?> GetBySlugAsync(string slug);
        Task<IEnumerable<CategoryDto>> GetAllAsync();
        Task<IEnumerable<CategoryDto>> GetAllActiveAsync();
        Task<PagedResult<CategoryDto>> GetAllPagedAsync(PaginationParams paginationParams);
        Task<IEnumerable<CategoryDto>> GetChildCategoriesAsync(int parentId);
        Task<CategoryDto> CreateAsync(CategoryCreateDto createDto);
        Task<CategoryDto> UpdateAsync(int categoryId, CategoryUpdateDto updateDto);
        Task<bool> DeleteAsync(int categoryId);
    }
}