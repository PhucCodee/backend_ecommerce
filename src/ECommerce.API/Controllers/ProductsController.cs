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

        #region Public Endpoints

        /// <summary>
        /// Get products with filtering and sorting
        /// </summary>
        /// <remarks>
        /// Sort options: price, name, a-z, rating, popularity, views, sales, newest, oldest
        /// 
        /// Examples:
        /// - /api/products?sortBy=price&amp;desc=false (cheapest first)
        /// - /api/products?sortBy=price&amp;desc=true (expensive first)
        /// - /api/products?sortBy=name (A to Z)
        /// - /api/products?sortBy=name&amp;desc=true (Z to A)
        /// - /api/products?minPrice=10&amp;maxPrice=100
        /// - /api/products?categoryId=5&amp;brand=Apple
        /// - /api/products?search=phone&amp;inStock=true
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams query)
        {
            var products = await _productService.GetFilteredAsync(query);
            return Ok(ApiResponse<PagedResult<ProductDetailDto>>.Ok(products));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductDetailDto>.Ok(product));
        }

        [HttpGet("{id:int}/variants")]
        public async Task<IActionResult> GetVariants(int id)
        {
            var variants = await _productService.GetVariantsAsync(id);
            return Ok(ApiResponse<object>.Ok(variants));
        }

        #endregion

        #region Admin Endpoints

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto createDto)
        {
            var sellerId = createDto.SellerId ?? GetCurrentUserId();
            var product = await _productService.CreateAsync(createDto, sellerId);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductDetailDto>.Ok(product, "Product created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto updateDto)
        {
            var product = await _productService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<ProductDetailDto>.Ok(product, "Product updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            await _productService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { id }, "Product deleted successfully"));
        }

        #endregion

        #region Seller Endpoints

        [HttpGet("seller")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> GetMyProducts([FromQuery] ProductQueryParams query)
        {
            var sellerId = GetCurrentUserId();
            query.SellerId = sellerId; // Force seller filter
            var products = await _productService.GetFilteredAsync(query);
            return Ok(ApiResponse<PagedResult<ProductDetailDto>>.Ok(products));
        }

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

        [HttpPut("seller/{id:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> UpdateAsSeller(int id, [FromBody] ProductUpdateDto updateDto)
        {
            var sellerId = GetCurrentUserId();
            var product = await _productService.UpdateSellerProductAsync(id, sellerId, updateDto);
            return Ok(ApiResponse<ProductDetailDto>.Ok(product, "Product updated successfully"));
        }

        [HttpDelete("seller/{id:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> DeleteAsSeller(int id)
        {
            var sellerId = GetCurrentUserId();
            await _productService.DeleteSellerProductAsync(id, sellerId);
            return Ok(ApiResponse<object>.Ok(new { id }, "Product deleted successfully"));
        }

        #endregion

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedException("Invalid user token");
            return userId;
        }
    }
}