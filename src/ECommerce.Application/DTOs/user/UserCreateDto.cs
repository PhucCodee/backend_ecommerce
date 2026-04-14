using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.user
{
    public class UserCreateDto
    {
        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(3)]
        public required string Username { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }

        [Required]
        public required string FirstName { get; set; }

        [Required]
        public required string LastName { get; set; }

        [Required]
        public string? Phone { get; set; }

        public int[]? Roles { get; set; }
    }
}

