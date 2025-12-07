using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.auth
{
    public class RefreshTokenDto
    {
        [Required]
        public required string RefreshToken { get; set; }
    }
}