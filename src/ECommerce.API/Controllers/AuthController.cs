using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "User registered successfully"));
        }

        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login successful"));
        }

        [EnableRateLimiting("AuthPolicy")]
        [HttpPost("change-password")]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _authService.ChangePasswordAsync(
                userId,
                dto.CurrentPassword,
                dto.NewPassword
            );
            return Ok(ApiResponse<AuthOperationResultDto>.Ok(result, result.Message));
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
