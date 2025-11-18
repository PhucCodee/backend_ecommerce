using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserProfile
{
    public int ProfileId { get; set; }

    public int UserId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public UserGender? Gender { get; set; }

    public string AvatarUrl { get; set; }

    public string Bio { get; set; }

    public Language PrefferedLanguage { get; set; }

    public Currency PreferredCurrency { get; set; }

    public string Timezone { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; }
}
