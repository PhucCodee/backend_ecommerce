using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.DTOs.address;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.Authenticated)]
    public class AddressesController(IAddressService addressService) : ControllerBase
    {
        private readonly IAddressService _addressService = addressService;

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var userId = GetCurrentUserId();
            var addresses = await _addressService.GetMyAddressesAsync(userId);
            return Ok(ApiResponse<IEnumerable<AddressDto>>.Ok(addresses));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddressCreateDto createDto)
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.CreateAsync(userId, createDto);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<AddressDto>.Ok(address, "Address created successfully"));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddressUpdateDto updateDto)
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.UpdateAsync(userId, id, updateDto);
            return Ok(ApiResponse<AddressDto>.Ok(address, "Address updated successfully"));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            await _addressService.DeleteAsync(userId, id);
            return Ok(ApiResponse<object>.Ok(new { id }, "Address deleted successfully"));
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