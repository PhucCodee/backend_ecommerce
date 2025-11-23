using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.common;
using ECommerce.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                var response = ApiResponse<AuthResponseDto>.SuccessResponse(result, "User registered successfully", 201);
                return Created("", response);
            }
            catch (InvalidOperationException ex)
            {
                var response = ApiResponse<AuthResponseDto>.Failure(ex.Message, 400);
                return BadRequest(response);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _authService.LoginAsync(loginDto);
                var response = ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful");
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                var response = ApiResponse<AuthResponseDto>.Failure(ex.Message, 401);
                return Unauthorized(response);
            }
        }
    }
}