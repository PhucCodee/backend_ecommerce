using System;
using ECommerce.Application.DTOs.user;

namespace ECommerce.Application.DTOs.auth
{
    public class AuthResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required UserDto User { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
