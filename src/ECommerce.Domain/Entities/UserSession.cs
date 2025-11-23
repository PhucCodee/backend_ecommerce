using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserSession
{
    public int SessionId { get; set; }

    public int UserId { get; set; }

    public required string AccessTokenHash { get; set; }

    public required string RefreshTokenHash { get; set; }

    public required string IpAddress { get; set; }

    public required string UserAgent { get; set; }

    public DeviceType Type { get; set; }

    public required string DeviceName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastActivityAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public required virtual User User { get; set; }

    public static UserSession CreateDefault(User user, string accessTokenHash, string refreshTokenHash, int expirationDays = 7)
    {
        return new UserSession
        {
            User = user,
            AccessTokenHash = accessTokenHash,
            RefreshTokenHash = refreshTokenHash,
            IpAddress = string.Empty,
            UserAgent = string.Empty,
            Type = DeviceType.desktop,
            DeviceName = string.Empty,
            CreatedAt = DateTime.UtcNow,
            LastActivityAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
        };
    }
}
