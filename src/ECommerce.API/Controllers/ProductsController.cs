using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Exceptions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

        // Get all products (public)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams, [FromQuery] bool? primaryOnly = null)
        {
            var products = await _productService.GetAllPagedAsync(paginationParams, primaryOnly);
            return Ok(ApiResponse<PagedResult<ProductDetailDto>>.Ok(products));
        }

        // Get only primary/parent products (public - for buyer homepage)
        [HttpGet("primary")]
        public async Task<IActionResult> GetPrimaryProducts([FromQuery] PaginationParams paginationParams)
        {
            var products = await _productService.GetAllPagedAsync(paginationParams, primaryOnly: true);
            return Ok(ApiResponse<PagedResult<ProductDetailDto>>.Ok(products));
        }

        // Get product by id (public)
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductDetailDto>.Ok(product));
        }

        // Get variants of a product (public)
        [HttpGet("{id:int}/variants")]
        public async Task<IActionResult> GetVariants(int id)
        {
            var variants = await _productService.GetVariantsAsync(id);
            return Ok(ApiResponse<object>.Ok(variants));
        }

        // Create a new product (Admin only)
        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto createDto)
        {
            var sellerId = createDto.SellerId > 0 ? createDto.SellerId : GetCurrentUserId();
            var product = await _productService.CreateAsync(createDto, sellerId);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductDetailDto>.Ok(product, "Product created successfully"));
        }

        // Update product (Admin only)
        [HttpPut("{id}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto updateDto)
        {
            var product = await _productService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<ProductDetailDto>.Ok(product, "Product updated successfully"));
        }

        // Delete product (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { id }, "Product deleted successfully"));
        }

        // Get current seller's products
        [HttpGet("me/products")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> GetMyProducts([FromQuery] PaginationParams paginationParams)
        {
            var sellerId = GetCurrentUserId();
            var products = await _productService.GetBySellerPagedAsync(sellerId, paginationParams);
            return Ok(ApiResponse<PagedResult<ProductDetailDto>>.Ok(products));
        }

        // Create product as seller
        [HttpPost("seller")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> CreateAsSeller([FromBody] ProductCreateDto createDto)
        {
            var sellerId = GetCurrentUserId();
            var product = await _productService.CreateAsync(createDto, sellerId);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductDetailDto>.Ok(product, "Product created successfully"));
        }

        // Update seller's own product
        [HttpPut("seller/{id}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> UpdateAsSeller(int id, [FromBody] ProductUpdateDto updateDto)
        {
            var sellerId = GetCurrentUserId();
            var product = await _productService.UpdateSellerProductAsync(id, sellerId, updateDto);
            return Ok(ApiResponse<ProductDetailDto>.Ok(product, "Product updated successfully"));
        }

        // Delete seller's own product
        [HttpDelete("seller/{id}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> DeleteAsSeller(int id)
        {
            var sellerId = GetCurrentUserId();
            await _productService.DeleteSellerProductAsync(id, sellerId);
            return Ok(ApiResponse<object>.Ok(new { id }, "Product deleted successfully"));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedException("Invalid user token");
            return userId;
        }
    }
}