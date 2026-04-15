using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Exceptions;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProductSkusController(
        IProductSkuQueryService productSkuQueryService,
        IProductSkuService productSkuService) : ControllerBase
    {
        private readonly IProductSkuQueryService _productSkuQueryService = productSkuQueryService;
        private readonly IProductSkuService _productSkuService = productSkuService;

        #region Public Endpoints

        [HttpGet("products/{productId:int}/skus")]
        public async Task<IActionResult> GetByProductId(int productId, [FromQuery] PaginationParams paginationParams)
        {
            var skus = await _productSkuQueryService.GetByProductIdPagedAsync(productId, paginationParams);
            return Ok(ApiResponse<PagedResult<ProductSkuDto>>.Ok(skus));
        }

        [HttpGet("skus/{skuId:int}")]
        public async Task<IActionResult> GetById(int skuId)
        {
            var sku = await _productSkuQueryService.GetByIdAsync(skuId);
            return Ok(ApiResponse<ProductSkuDto>.Ok(sku));
        }

        #endregion

        #region Admin Endpoints

        [HttpPut("skus/{skuId:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int skuId, [FromBody] ProductSkuUpdateDto updateDto)
        {
            var sku = await _productSkuService.UpdateAsync(skuId, updateDto);
            return Ok(ApiResponse<ProductSkuDto>.Ok(sku, "Product SKU updated successfully"));
        }

        [HttpDelete("skus/{skuId:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int skuId)
        {
            await _productSkuService.DeleteAsync(skuId);
            return Ok(ApiResponse<object>.Ok(new { skuId }, "Product SKU deleted successfully"));
        }

        #endregion

        #region Seller Endpoints

        [HttpGet("skus/seller")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> GetSellerSkus([FromQuery] PaginationParams paginationParams)
        {
            var sellerId = GetCurrentUserId();
            var skus = await _productSkuQueryService.GetBySellerPagedAsync(sellerId, paginationParams);
            return Ok(ApiResponse<PagedResult<ProductSkuDto>>.Ok(skus));
        }

        [HttpPost("skus/seller")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> CreateAsSeller([FromBody] ProductSkuCreateDto createDto)
        {
            var sellerId = GetCurrentUserId();
            var sku = await _productSkuService.CreateAsync(createDto, sellerId);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductSkuDto>.Ok(sku, "Product SKU created successfully"));
        }

        [HttpPut("skus/seller/{skuId:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> UpdateAsSeller(int skuId, [FromBody] ProductSkuUpdateDto updateDto)
        {
            var sellerId = GetCurrentUserId();
            var sku = await _productSkuService.UpdateAsync(skuId, updateDto, sellerId);
            return Ok(ApiResponse<ProductSkuDto>.Ok(sku, "Product SKU updated successfully"));
        }

        [HttpDelete("skus/seller/{skuId:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> DeleteAsSeller(int skuId)
        {
            var sellerId = GetCurrentUserId();
            await _productSkuService.DeleteAsync(skuId, sellerId);
            return Ok(ApiResponse<object>.Ok(new { skuId }, "Product SKU deleted successfully"));
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