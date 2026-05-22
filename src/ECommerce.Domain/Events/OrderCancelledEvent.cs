using System;

namespace ECommerce.Domain.Events;

public record OrderCancelledEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "order.cancelled";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required int OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required int UserId { get; init; }
    public string? Notes { get; init; }
}
