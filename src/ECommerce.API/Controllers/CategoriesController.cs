using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.DTOs.category;
using ECommerce.Application.Interfaces;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController(ICategoryService categoryService) : ControllerBase
    {
        private readonly ICategoryService _categoryService = categoryService;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
        {
            var categories = await _categoryService.GetAllPagedAsync(paginationParams);
            return Ok(ApiResponse<PagedResult<CategoryDto>>.Ok(categories));
        }

        [HttpGet("core")]
        public async Task<IActionResult> GetCoreCategories([FromQuery] PaginationParams paginationParams)
        {
            var categories = await _categoryService.GetCoreCategoriesPagedAsync(paginationParams);
            return Ok(ApiResponse<PagedResult<CategoryDto>>.Ok(categories));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(ApiResponse<CategoryDto>.Ok(category));
        }

        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            var category = await _categoryService.GetBySlugAsync(slug);
            return Ok(ApiResponse<CategoryDto>.Ok(category));
        }

        [HttpGet("{id:int}/children")]
        public async Task<IActionResult> GetChildren(int id)
        {
            var categories = await _categoryService.GetChildCategoriesAsync(id);
            return Ok(ApiResponse<object>.Ok(categories));
        }

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto createDto)
        {
            var category = await _categoryService.CreateAsync(createDto);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<CategoryDto>.Ok(category, "Category created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto updateDto)
        {
            var category = await _categoryService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<CategoryDto>.Ok(category, "Category updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { id }, "Category deleted successfully"));
        }
    }
}