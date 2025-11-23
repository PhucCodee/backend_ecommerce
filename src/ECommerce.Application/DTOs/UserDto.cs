using System;

namespace ECommerce.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; } // Unique identifier for the user
        public required string Username { get; set; } // Username of the user
        public required string Email { get; set; } // Email address of the user
        public required string Password { get; set; } // Password for the user account
        public required string FirstName { get; set; } // First name of the user
        public required string LastName { get; set; } // Last name of the user
        public DateTime CreatedAt { get; set; } // Date and time when the user was created
        public DateTime UpdatedAt { get; set; } // Date and time when the user was last updated
    }
}