using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.DTOs.inventory;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.SellerOnly)]
    public class InventoriesController(IInventoryService inventoryService) : ControllerBase
    {
        private readonly IInventoryService _inventoryService = inventoryService;

        [HttpPut("skus/{skuId:int}")]
        public async Task<IActionResult> UpdateBySku(
            int skuId,
            [FromBody] InventoryUpdateDto updateDto
        )
        {
            var sellerId = GetCurrentUserId();
            var inventory = await _inventoryService.UpdateAsync(skuId, updateDto, sellerId);
            return Ok(ApiResponse<InventoryDto>.Ok(inventory, "Inventory updated successfully"));
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
