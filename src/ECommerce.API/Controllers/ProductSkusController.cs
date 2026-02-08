using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.DTOs.productsku;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Exceptions;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProductSkusController(IProductSkuService productSkuService) : ControllerBase
    {
        private readonly IProductSkuService _productSkuService = productSkuService;

        #region Public Endpoints

        [HttpGet("products/{productId:int}/skus")]
        public async Task<IActionResult> GetByProductId(int productId, [FromQuery] PaginationParams paginationParams)
        {
            var skus = await _productSkuService.GetByProductIdPagedAsync(productId, paginationParams);
            return Ok(ApiResponse<PagedResult<ProductSkuDetailDto>>.Ok(skus));
        }

        [HttpGet("skus/{skuId:int}")]
        public async Task<IActionResult> GetById(int skuId)
        {
            var sku = await _productSkuService.GetByIdAsync(skuId);
            return Ok(ApiResponse<ProductSkuDetailDto>.Ok(sku));
        }

        #endregion

        #region Admin Endpoints

        [HttpPost("products/{productId:int}/skus")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Create(int productId, [FromBody] ProductSkuCreateDto createDto)
        {
            createDto.ProductId = productId;
            var sku = await _productSkuService.CreateAsync(createDto);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductSkuDetailDto>.Ok(sku, "Product SKU created successfully"));
        }

        [HttpPut("skus/{skuId:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int skuId, [FromBody] ProductSkuUpdateDto updateDto)
        {
            var sku = await _productSkuService.UpdateAsync(skuId, updateDto);
            return Ok(ApiResponse<ProductSkuDetailDto>.Ok(sku, "Product SKU updated successfully"));
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

        [HttpGet("seller/skus")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> GetSellerSkus([FromQuery] PaginationParams paginationParams)
        {
            var sellerId = GetCurrentUserId();
            var skus = await _productSkuService.GetBySellerPagedAsync(sellerId, paginationParams);
            return Ok(ApiResponse<PagedResult<ProductSkuDetailDto>>.Ok(skus));
        }

        [HttpPost("seller/products/{productId:int}/skus")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> CreateAsSeller(int productId, [FromBody] ProductSkuCreateDto createDto)
        {
            var sellerId = GetCurrentUserId();
            createDto.ProductId = productId;
            var sku = await _productSkuService.CreateSellerSkuAsync(createDto, sellerId);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductSkuDetailDto>.Ok(sku, "Product SKU created successfully"));
        }

        [HttpPut("seller/skus/{skuId:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> UpdateAsSeller(int skuId, [FromBody] ProductSkuUpdateDto updateDto)
        {
            var sellerId = GetCurrentUserId();
            var sku = await _productSkuService.UpdateSellerSkuAsync(skuId, sellerId, updateDto);
            return Ok(ApiResponse<ProductSkuDetailDto>.Ok(sku, "Product SKU updated successfully"));
        }

        [HttpDelete("seller/skus/{skuId:int}")]
        [Authorize(Policy = Policies.SellerOnly)]
        public async Task<IActionResult> DeleteAsSeller(int skuId)
        {
            var sellerId = GetCurrentUserId();
            await _productSkuService.DeleteSellerSkuAsync(skuId, sellerId);
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