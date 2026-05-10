using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Cart
{
    public int CartId { get; set; }

    public int? UserId { get; set; }

    public string? SessionId { get; set; }

    public CartStatus Status { get; set; }

    public decimal Subtotal { get; set; }

    public int TotalItems { get; set; }

    public string? IpAddress { get; set; }

    public DateTime? AbandonedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = [];

    public static Cart CreateDefault(
        int? userId = null,
        string? sessionId = null,
        string? ipAddress = null
    )
    {
        return new Cart
        {
            UserId = userId,
            SessionId = sessionId,
            Status = CartStatus.active,
            Subtotal = 0,
            TotalItems = 0,
            IpAddress = ipAddress,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void RecalculateTotals()
    {
        Subtotal = 0;
        TotalItems = 0;

        foreach (var item in CartItems)
        {
            Subtotal += item.PriceSnapshot * item.Quantity;
            TotalItems += item.Quantity;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
