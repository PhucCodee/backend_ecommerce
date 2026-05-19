using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(
        IProductQueryService productQueryService,
        IProductService productService
    ) : ControllerBase
    {
        private readonly IProductQueryService _productQueryService = productQueryService;
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
        [EnableRateLimiting("ApiPolicy")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams query)
        {
            var products = await _productQueryService.GetPublicFilteredAsync(query);
            return Ok(ApiResponse<PagedResult<PublicProductSummaryDto>>.Ok(products));
        }

        [EnableRateLimiting("ApiPolicy")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productQueryService.GetByIdAsync(id);
            return Ok(ApiResponse<ProductDetailDto>.Ok(product));
        }

        #endregion

        #region Admin Endpoints

        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto updateDto)
        {
            var product = await _productService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated successfully"));
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
        public async Task<IActionResult> GetSellerProducts(
            [FromQuery] ProductQueryParams productQueryParams
        )
        {
            var sellerId = GetCurrentUserId();
            productQueryParams.SellerId = sellerId;
            productQueryParams.CategoryIds.Clear();
            productQueryParams.IncludeSuspended = true;
            var products = await _productQueryService.GetFilteredAsync(productQueryParams);
            return Ok(ApiResponse<PagedResult<ProductSummaryDto>>.Ok(products));
        }

        [HttpPost("seller")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> CreateAsSeller([FromBody] ProductCreateDto createDto)
        {
            var sellerId = GetCurrentUserId();
            var product = await _productService.CreateAsync(createDto, sellerId);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductDto>.Ok(product, "Product created successfully")
            );
        }

        [HttpPut("seller/{id:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> UpdateAsSeller(
            int id,
            [FromBody] ProductUpdateDto updateDto
        )
        {
            var sellerId = GetCurrentUserId();
            var product = await _productService.UpdateAsync(id, updateDto, sellerId);
            return Ok(ApiResponse<ProductDto>.Ok(product, "Product updated successfully"));
        }

        [HttpDelete("seller/{id:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> DeleteAsSeller(int id)
        {
            var sellerId = GetCurrentUserId();
            await _productService.DeleteAsync(id, sellerId);
            return Ok(ApiResponse<object>.Ok(new { id }, "Product deleted successfully"));
        }

        [HttpPut("seller/{id:int}/restore")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> RestoreAsSeller(int id)
        {
            var sellerId = GetCurrentUserId();
            var product = await _productService.RestoreAsync(id, sellerId);
            return Ok(ApiResponse<ProductDto>.Ok(product, "Product restored successfully"));
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
