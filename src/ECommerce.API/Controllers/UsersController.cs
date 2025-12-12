using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Common.Responses;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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
            return Ok(ApiResponse<IEnumerable<UserProfileDto>>.SuccessResponse(users));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<UserProfileDto>.Failure("User not found", StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<UserProfileDto>.SuccessResponse(user));
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UserCreateDto createDto)
        {
            try
            {
                var user = await _userService.CreateAsync(createDto);
                var response = ApiResponse<UserProfileDto>.SuccessResponse(user, "User created", StatusCodes.Status201Created);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (InvalidOperationException ex)
            {
                var response = ApiResponse<UserProfileDto>.Failure(ex.Message, StatusCodes.Status400BadRequest);
                return BadRequest(response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto updateDto)
        {
            try
            {
                var user = await _userService.UpdateAsync(id, updateDto);
                if (user == null)
                {
                    return NotFound(ApiResponse<UserProfileDto>.Failure("User not found", StatusCodes.Status404NotFound));
                }

                var response = ApiResponse<UserProfileDto>.SuccessResponse(user, "User updated");
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var response = ApiResponse<UserProfileDto>.Failure(ex.Message, StatusCodes.Status400BadRequest);
                return BadRequest(response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _userService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<object>.Failure("User not found", StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, "User deleted"));
        }

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