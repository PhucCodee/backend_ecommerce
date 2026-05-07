using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Authorization;
using Microsoft.AspNetCore.Http;
using ECommerce.Application.Exceptions;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(IUserService userService) : ControllerBase
    {
        // Get current user's profile
        [HttpGet("profile")]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var result = await userService.GetProfileAsync(userId);
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }

        // Update current user's profile
        [HttpPut("profile")]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto updateDto)
        {
            var userId = GetCurrentUserId();
            var user = await userService.UpdateProfileAsync(userId, updateDto);
            return Ok(ApiResponse<UserProfileDto>.Ok(user, "Profile updated successfully"));
        }

        // Get all users
        [HttpGet]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams paginationParams)
        {
            var users = await userService.GetAllPagedAsync(paginationParams);
            return Ok(ApiResponse<PagedResult<UserProfileDto>>.Ok(users));
        }

        // Get user by id
        [HttpGet("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await userService.GetByIdAsync(id);
            return Ok(ApiResponse<UserProfileDto>.Ok(user));
        }

        // Create a new user
        [HttpPost]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] UserCreateDto createDto)
        {
            var user = await userService.CreateAsync(createDto);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<UserProfileDto>.Ok(user, "User created successfully")
            );
        }

        // Update user
        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto updateDto)
        {
            var user = await userService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<UserProfileDto>.Ok(user, "User updated successfully"));
        }

        // Delete user
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.AdminOnly)]
        public async Task<IActionResult> Delete(int id)
        {
            await userService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { id }, "User deleted successfully"));
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