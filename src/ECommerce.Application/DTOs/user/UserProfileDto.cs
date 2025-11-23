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
        public bool EmailVerified { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; }
    }
}