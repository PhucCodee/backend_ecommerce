using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Worker;

public sealed class InventoryConsumer(
    ApplicationDbContext db,
    IPublishEndpoint publishEndpoint,
    ILogger<InventoryConsumer> logger
) : IConsumer<OrderCreatedEvent>
{
    private readonly ApplicationDbContext _db = db;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<InventoryConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;
        const string processedBy = "inventory-consumer";

        var alreadyProcessed = await _db.ProcessedEvents.AnyAsync(
            x => x.EventId == message.EventId && x.ProcessedBy == processedBy,
            context.CancellationToken
        );

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "InventoryConsumer skipped already processed event {EventId}",
                message.EventId
            );
            return;
        }

        var requestedBySku = message
            .Items.GroupBy(i => i.SkuId)
            .Select(g => new { SkuId = g.Key, Requested = g.Sum(x => x.Quantity) })
            .ToList();

        var skuIds = requestedBySku.Select(x => x.SkuId).ToList();
        var inventoryBySku = await _db
            .Inventories.Where(i => skuIds.Contains(i.SkuId))
            .ToDictionaryAsync(i => i.SkuId, context.CancellationToken);

        var failures = new List<InventoryReservationFailedItemEvent>();

        foreach (var req in requestedBySku)
        {
            if (!inventoryBySku.TryGetValue(req.SkuId, out var inventory))
            {
                failures.Add(
                    new InventoryReservationFailedItemEvent
                    {
                        SkuId = req.SkuId,
                        RequestedQuantity = req.Requested,
                        AvailableQuantity = 0,
                        Error = "Inventory record not found",
                    }
                );
                continue;
            }

            if (inventory.QuantityAvailable < req.Requested)
            {
                failures.Add(
                    new InventoryReservationFailedItemEvent
                    {
                        SkuId = req.SkuId,
                        RequestedQuantity = req.Requested,
                        AvailableQuantity = inventory.QuantityAvailable,
                        Error = "Insufficient stock",
                    }
                );
            }
        }

        if (failures.Count > 0)
        {
            await _publishEndpoint.Publish(
                new InventoryReservationFailedEvent
                {
                    OrderId = message.OrderId,
                    OrderNumber = message.OrderNumber,
                    UserId = message.UserId,
                    SourceEventId = message.EventId,
                    Reason = "One or more SKUs do not have enough stock",
                    Failures = failures,
                },
                context.CancellationToken
            );

            _db.ProcessedEvents.Add(
                new ProcessedEvent
                {
                    EventId = message.EventId,
                    EventType = message.EventType,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = processedBy,
                }
            );

            await _db.SaveChangesAsync(context.CancellationToken);

            _logger.LogWarning(
                "Inventory reservation failed for order {OrderId}. FailedItems={FailedCount}",
                message.OrderId,
                failures.Count
            );

            return;
        }

        var reservedItems = new List<InventoryReservedItemEvent>();

        foreach (var req in requestedBySku)
        {
            var inventory = inventoryBySku[req.SkuId];
            var beforeAvailable = inventory.QuantityAvailable;
            var beforeReserved = inventory.QuantityReserved;

            inventory.QuantityAvailable -= req.Requested;
            inventory.QuantityReserved += req.Requested;
            inventory.UpdatedAt = DateTime.UtcNow;

            _db.InventoryHistories.Add(
                new InventoryHistory
                {
                    Inventory = inventory,
                    ChangedByNavigation = null!,
                    ChangeType = "reserve",
                    QuantityChange = -req.Requested,
                    QuantityBefore = beforeAvailable,
                    QuantityAfter = inventory.QuantityAvailable,
                    ReferenceType = "order",
                    ReferenceId = message.OrderId,
                    Notes = $"Reserved for order {message.OrderNumber}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }
            );

            reservedItems.Add(
                new InventoryReservedItemEvent
                {
                    SkuId = req.SkuId,
                    ReservedQuantity = req.Requested,
                    RemainingAvailable = inventory.QuantityAvailable,
                    TotalReserved = inventory.QuantityReserved,
                }
            );
        }

        await _publishEndpoint.Publish(
            new InventoryReservedEvent
            {
                OrderId = message.OrderId,
                OrderNumber = message.OrderNumber,
                UserId = message.UserId,
                SourceEventId = message.EventId,
                Items = reservedItems,
            },
            context.CancellationToken
        );

        _db.ProcessedEvents.Add(
            new ProcessedEvent
            {
                EventId = message.EventId,
                EventType = message.EventType,
                ProcessedAt = DateTime.UtcNow,
                ProcessedBy = processedBy,
            }
        );

        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Inventory reserved for order {OrderId}. ItemCount={ItemCount}",
            message.OrderId,
            reservedItems.Count
        );
    }
}
