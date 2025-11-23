using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs.common;
using ECommerce.Application.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ECommerce.Application.DTOs.user;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    var errorResponse = ApiResponse<UserProfileDto>.Failure("Invalid user token", 401);
                    return Unauthorized(errorResponse);
                }

                var result = await _userService.GetProfileAsync(userId);
                var response = ApiResponse<UserProfileDto>.SuccessResponse(result);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var response = ApiResponse<UserProfileDto>.Failure(ex.Message, 404);
                return NotFound(response);
            }
        }
    }
}