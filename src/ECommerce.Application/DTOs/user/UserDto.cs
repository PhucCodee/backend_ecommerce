namespace ECommerce.Application.DTOs.user
{
    public class UserDto
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string Username { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string Status { get; set; } = "Active";
        public string[] Roles { get; set; } = [];
    }
}
