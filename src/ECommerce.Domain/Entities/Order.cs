using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class Order
{
    public int OrderId { get; set; }

    public required string OrderNumber { get; set; } = string.Empty;

    public int UserId { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.created;

    public decimal Subtotal { get; set; }

    public decimal ShippingFee { get; set; }

    public decimal TaxAmount { get; set; }

    public int? CouponId { get; set; }

    public string? CouponCode { get; set; }

    public decimal CouponDiscount { get; set; }

    public decimal TotalAmount { get; set; }

    public Currency PreferredCurrency { get; set; } = Currency.vnd;

    public string? CustomerNotes { get; set; }

    public string? AdminNotes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual ICollection<EventLog> EventLogs { get; set; } = [];

    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    public virtual ICollection<OrderPayment> OrderPayments { get; set; } = [];

    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = [];

    public virtual ICollection<Review> Reviews { get; set; } = [];

    public virtual OrderFulfillment? OrderFulfillment { get; set; }

    public virtual OrderShipping? OrderShipping { get; set; }

    public required virtual User User { get; set; }

    public virtual Coupon? Coupon { get; set; }
}