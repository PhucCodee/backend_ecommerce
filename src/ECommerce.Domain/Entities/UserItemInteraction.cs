using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class UserItemInteraction
{
    public int InteractionId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public string InteractionType { get; set; }

    public int Weight { get; set; }

    public string SessionId { get; set; }

    public string ReferrerUrl { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; }

    public virtual User User { get; set; }
}
