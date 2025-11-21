using System;
using System.Collections.Generic;
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

    public required virtual User GrantedByNavigation { get; set; }

    public required virtual User User { get; set; }
}
