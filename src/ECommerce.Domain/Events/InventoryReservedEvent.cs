using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Events;

public record InventoryReservedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "inventory.reserved";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required int OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required int UserId { get; init; }
    public required Guid SourceEventId { get; init; }
    public required IReadOnlyList<InventoryReservedItemEvent> Items { get; init; }
}

public record InventoryReservedItemEvent
{
    public required int SkuId { get; init; }
    public required int ReservedQuantity { get; init; }
    public required int RemainingAvailable { get; init; }
    public required int TotalReserved { get; init; }
}