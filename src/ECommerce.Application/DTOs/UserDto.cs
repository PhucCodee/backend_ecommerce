using System;

namespace ECommerce.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; } // Unique identifier for the user
        public string Username { get; set; } // Username of the user
        public string Email { get; set; } // Email address of the user
        public string Password { get; set; } // Password for the user account
        public string FirstName { get; set; } // First name of the user
        public string LastName { get; set; } // Last name of the user
        public DateTime CreatedAt { get; set; } // Date and time when the user was created
        public DateTime UpdatedAt { get; set; } // Date and time when the user was last updated
    }
}