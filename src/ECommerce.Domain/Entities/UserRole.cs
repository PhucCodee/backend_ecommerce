using System;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserRole
{
    public int UserRoleId { get; set; }

    public int UserId { get; set; }

    public UserRoleType Role { get; set; }

    public DateTime GrantedAt { get; set; }

    public int? GrantedBy { get; set; }

    public DateTime? RevokedAt { get; set; }

    public virtual User? GrantedByNavigation { get; set; }

    public virtual required User User { get; set; }

    public static UserRole CreateDefault(User user, UserRoleType role, User? grantedBy = null)
    {
        return new UserRole
        {
            User = user,
            Role = role,
            GrantedAt = DateTime.UtcNow,
            GrantedByNavigation = grantedBy,
        };
    }

    public bool IsActive() => RevokedAt == null;
}
