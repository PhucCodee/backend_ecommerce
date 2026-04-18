using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.DTOs.coupon;
using ECommerce.Application.Interfaces;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController(ICouponService couponService) : ControllerBase
    {
        private readonly ICouponService _couponService = couponService;

        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] CouponCreateDto createDto)
        {
            var coupon = await _couponService.CreateAsync(createDto);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<CouponDto>.Ok(coupon, "Coupon created successfully"));
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] CouponUpdateDto updateDto)
        {
            var coupon = await _couponService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<CouponDto>.Ok(coupon, "Coupon updated successfully"));
        }

        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            await _couponService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { id }, "Coupon deleted successfully"));
        }
    }
}