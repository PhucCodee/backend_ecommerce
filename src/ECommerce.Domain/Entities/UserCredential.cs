using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class UserCredential
{
    public int CredentialId { get; set; }

    public int UserId { get; set; }

    public required string PasswordHash { get; set; } = string.Empty;

    public required string PasswordSalt { get; set; } = string.Empty;

    public DateTime PasswordUpdatedAt { get; set; }

    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LastFailedAttemptAt { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public required string LastLoginIp { get; set; } = string.Empty;

    public required string ResetTokenHash { get; set; } = string.Empty;

    public DateTime? ResetTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual required User User { get; set; }

    public static UserCredential CreateDefault(User user, string passwordHash, string passwordSalt)
    {
        return new UserCredential
        {
            User = user,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            PasswordUpdatedAt = DateTime.UtcNow,
            FailedLoginAttempts = 0,
            LastLoginIp = string.Empty,
            ResetTokenHash = string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
