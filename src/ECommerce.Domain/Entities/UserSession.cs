using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserSession
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public string AccessTokenHash { get; set; }

    public string RefreshTokenHash { get; set; }

    public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public DeviceType Type { get; set; }

    public string DeviceName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastActivityAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public virtual User User { get; set; }
}
