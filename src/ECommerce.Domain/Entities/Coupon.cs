using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public class Coupon
{
    public int CouponId { get; set; }

    public required string Code { get; set; }

    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; }

    public decimal DiscountValue { get; set; }

    public decimal? MinOrderAmount { get; set; }

    public int? UsageLimit { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidUntil { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Order> Orders { get; set; } = [];
}