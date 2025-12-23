using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.auth
{
    public class LoginDto
    {
        [Required]
        public required string Identifier { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}