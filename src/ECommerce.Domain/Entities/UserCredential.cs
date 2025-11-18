using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class UserCredential
{
    public int CredentialId { get; set; }

    public int UserId { get; set; }

    public string PasswordHash { get; set; }

    public string PasswordSalt { get; set; }

    public DateTime PasswordUpdatedAt { get; set; }

    public int FailedLoginAttempts { get; set; }

    public DateTime? LastFailedAttemptAt { get; set; }

    public DateTime? LockedUntil { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public string LastLoginIp { get; set; }

    public string ResetTokenHash { get; set; }

    public DateTime? ResetTokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; }
}
