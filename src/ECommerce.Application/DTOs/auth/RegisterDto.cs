using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.auth
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MinLength(3)]
        public required string Username { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        public string? Phone { get; set; }
    }
}