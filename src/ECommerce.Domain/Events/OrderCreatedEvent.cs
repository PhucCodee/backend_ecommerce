using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Events;

public record OrderCreatedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "order.created";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required int OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required int UserId { get; init; }
    public required decimal TotalAmount { get; init; }
    public required decimal Subtotal { get; init; }
    public required decimal ShippingFee { get; init; }
    public required decimal TaxAmount { get; init; }
    public string? CouponCode { get; init; }
    public decimal CouponDiscount { get; init; }
    public required IReadOnlyList<OrderItemEvent> Items { get; init; }
}

public record OrderItemEvent
{
    public required int SkuId { get; init; }
    public required string ProductName { get; init; }
    public required string Sku { get; init; }
    public required int SellerId { get; init; }
    public required int Quantity { get; init; }
    public required decimal UnitPrice { get; init; }
    public required decimal Subtotal { get; init; }
}
