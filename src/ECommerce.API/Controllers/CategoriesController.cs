using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Common.Responses;
using ECommerce.Domain.Repositories;
using System.Threading.Tasks;
using System.Linq;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController(ICategoryRepository categoryRepository) : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;

        /// <summary>
        /// Get all active categories for dropdown
        /// </summary>
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveCategories()
        {
            var categories = await _categoryRepository.GetAllActiveAsync();
            var result = categories.Select(c => new
            {
                categoryId = c.CategoryId,
                categoryName = c.CategoryName,
                slug = c.Slug,
                parentCategoryId = c.ParentCategoryId,
                description = c.Description,
                imageUrl = c.ImageUrl,
                isActive = c.IsActive
            });
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var result = categories.Select(c => new
            {
                categoryId = c.CategoryId,
                categoryName = c.CategoryName,
                slug = c.Slug,
                parentCategoryId = c.ParentCategoryId,
                description = c.Description,
                imageUrl = c.ImageUrl,
                displayOrder = c.DisplayOrder,
                isActive = c.IsActive,
                createdAt = c.CreatedAt,
                updatedAt = c.UpdatedAt
            });
            return Ok(ApiResponse<object>.Ok(result));
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
                return NotFound(ApiResponse<object>.Fail("Category not found"));

            var result = new
            {
                categoryId = category.CategoryId,
                categoryName = category.CategoryName,
                slug = category.Slug,
                parentCategoryId = category.ParentCategoryId,
                description = category.Description,
                imageUrl = category.ImageUrl,
                displayOrder = category.DisplayOrder,
                isActive = category.IsActive,
                createdAt = category.CreatedAt,
                updatedAt = category.UpdatedAt
            };
            return Ok(ApiResponse<object>.Ok(result));
        }
    }
}



