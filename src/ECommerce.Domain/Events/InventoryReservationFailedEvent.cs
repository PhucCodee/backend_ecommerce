using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Events;

public record InventoryReservationFailedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "inventory.reservation.failed";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required int OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required int UserId { get; init; }
    public required Guid SourceEventId { get; init; }
    public required string Reason { get; init; }
    public required IReadOnlyList<InventoryReservationFailedItemEvent> Failures { get; init; }
}

public record InventoryReservationFailedItemEvent
{
    public required int SkuId { get; init; }
    public required int RequestedQuantity { get; init; }
    public required int AvailableQuantity { get; init; }
    public required string Error { get; init; }
}