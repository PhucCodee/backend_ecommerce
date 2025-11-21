using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserProfile
{
    public int ProfileId { get; set; }

    public int UserId { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public UserGender? Gender { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public Language PreferredLanguage { get; set; }

    public Currency PreferredCurrency { get; set; }

    public string? Timezone { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public required virtual User User { get; set; }
}
