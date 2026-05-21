using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class UserItemInteraction
{
    public int InteractionId { get; set; }

    public int UserId { get; set; }

    public int ProductId { get; set; }

    public required string InteractionType { get; set; } = string.Empty;

    public int Weight { get; set; }

    public required string SessionId { get; set; } = string.Empty;

    public required string ReferrerUrl { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public virtual required Product Product { get; set; }

    public virtual required User User { get; set; }
}
