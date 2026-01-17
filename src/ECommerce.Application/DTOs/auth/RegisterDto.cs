using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ECommerce.Application.DTOs.auth
{
    public class RegisterDto : IValidatableObject
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [MaxLength(128, ErrorMessage = "Password cannot exceed 128 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public required string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [MinLength(1, ErrorMessage = "First name is required")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        [RegularExpression(@"^[\p{L}\s\-']+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and apostrophes")]

        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MinLength(1, ErrorMessage = "Last name is required")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        [RegularExpression(@"^[\p{L}\s\-']+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and apostrophes")]
        public required string LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^(0|\+84)(3[2-9]|5[6|8|9]|7[0|6-9]|8[1-5]|9[0-9])[0-9]{7}$", ErrorMessage = "Phone number must be a valid Vietnamese mobile number")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "You must accept the terms and conditions")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the terms and conditions")]
        public bool AcceptTerms { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Check for common passwords
            var commonPasswords = new[] { "password", "12345678", "qwerty", "abc12345" };
            if (commonPasswords.Any(cp => Password.Equals(cp, StringComparison.OrdinalIgnoreCase)))
            {
                yield return new ValidationResult(
                    "Password is too common. Please choose a stronger password.",
                    [nameof(Password)]
                );
            }

            // Check if password contains username
            if (Password.Contains(Username, StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Password cannot contain your username.",
                    [nameof(Password)]
                );
            }

            // Check if password contains email
            if (Password.Contains(Email.Split('@')[0], StringComparison.OrdinalIgnoreCase))
            {
                yield return new ValidationResult(
                    "Password cannot contain your email address.",
                    [nameof(Password)]
                );
            }

            // Validate username doesn't contain offensive words
            var offensiveWords = new[] { "admin", "root", "system", "support" };
            if (offensiveWords.Any(word => Username.Contains(word, StringComparison.OrdinalIgnoreCase)))
            {
                yield return new ValidationResult(
                    "Username contains restricted words.",
                    [nameof(Username)]
                );
            }
        }
    }
}