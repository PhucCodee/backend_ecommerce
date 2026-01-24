using System;

namespace ECommerce.Application.DTOs.user
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? Phone { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? PreferredLanguage { get; set; }
        public string? PreferredCurrency { get; set; }
        public string? Timezone { get; set; }
        public bool EmailVerified { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int[] Roles { get; set; } = Array.Empty<int>();
    }
}