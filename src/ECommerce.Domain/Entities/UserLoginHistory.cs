using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserLoginHistory
{
    public int LoginId { get; set; }

    public int? UserId { get; set; }

    public string Email { get; set; }

    public LoginStatus Status { get; set; }

    public string FailureReason { get; set; }

    public string IpAddress { get; set; }

    public string UserAgent { get; set; }

    public string LocationCountry { get; set; }

    public string LocationCity { get; set; }

    public bool IsSuspicious { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User User { get; set; }
}
