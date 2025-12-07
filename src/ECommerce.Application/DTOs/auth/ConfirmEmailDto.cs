using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.auth
{
    public class ConfirmEmailDto
    {
        [Required]
        public required string Token { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}