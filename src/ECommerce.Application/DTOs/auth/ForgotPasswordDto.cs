using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.auth
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
    }
}