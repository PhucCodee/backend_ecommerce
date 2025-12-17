using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Cart
{
    public int CartId { get; set; }

    public int? UserId { get; set; } // Nullable for guest carts

    public string? SessionId { get; set; } // For guest carts

    public CartStatus Status { get; set; }

    public decimal Subtotal { get; set; }

    public int TotalItems { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? AbandonedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = [];
}