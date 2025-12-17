using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Common.Responses;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ECommerce.Application.Common.Exceptions;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllAsync();
            return Ok(ApiResponse<IEnumerable<UserProfileDto>>.Ok(users));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(ApiResponse<UserProfileDto>.Ok(user));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UserCreateDto createDto)
        {
            var user = await _userService.CreateAsync(createDto);
            var response = ApiResponse<UserProfileDto>.Ok(user, "User created");
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto updateDto)
        {
            var user = await _userService.UpdateAsync(id, updateDto);
            return Ok(ApiResponse<UserProfileDto>.Ok(user, "User updated"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteAsync(id);
            return Ok(ApiResponse<object>.Ok(new { id }, "User deleted"));
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedException("Invalid user token");
            }

            var result = await _userService.GetProfileAsync(userId);
            return Ok(ApiResponse<UserProfileDto>.Ok(result));
        }
    }
}