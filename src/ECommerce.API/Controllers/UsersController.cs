using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using System;
using System.Threading.Tasks;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // POST: api/users/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDto)
        {
            // Placeholder: always succeed
            userDto.Id = Guid.NewGuid();
            return Ok(userDto);
        }

        // POST: api/users/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(UserDto userDto)
        {
            // Placeholder: always succeed
            return Ok(new { Message = "Login successful", User = userDto });
        }

        // GET: api/users/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Placeholder: return a dummy user
            var user = new UserDto
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            return Ok(user);
        }
    }
}