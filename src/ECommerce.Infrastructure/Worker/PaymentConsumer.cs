using System;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Worker;

public sealed class PaymentConsumer(
    ApplicationDbContext db,
    IPaymentGatewayClient paymentGatewayClient,
    IPublishEndpoint publishEndpoint,
    ILogger<PaymentConsumer> logger
) : IConsumer<InventoryReservedEvent>, IConsumer<InventoryReservationFailedEvent>
{
    private readonly ApplicationDbContext _db = db;
    private readonly IPaymentGatewayClient _paymentGatewayClient = paymentGatewayClient;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<PaymentConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<InventoryReservedEvent> context)
    {
        const string processedBy = "payment-consumer";
        var message = context.Message;

        if (await IsAlreadyProcessedAsync(message.EventId, processedBy, context.CancellationToken))
        {
            _logger.LogInformation(
                "PaymentConsumer skipped already processed event {EventId}",
                message.EventId
            );
            return;
        }

        var order = await _db.Orders.FirstOrDefaultAsync(
            o => o.OrderId == message.OrderId,
            context.CancellationToken
        );

        if (order is null)
        {
            await PublishFailedAsync(
                orderId: message.OrderId,
                orderNumber: message.OrderNumber,
                userId: message.UserId,
                sourceEventId: message.EventId,
                reason: "Order not found for payment",
                errorCode: "order_not_found",
                ct: context.CancellationToken
            );

            MarkProcessed(message.EventId, message.EventType, processedBy);
            await _db.SaveChangesAsync(context.CancellationToken);
            return;
        }

        var exists = await _db.OrderPayments.AnyAsync(
            p =>
                p.OrderId == order.OrderId
                && p.PaymentGateway == "zalopay"
                && (p.Status == PaymentStatus.pending || p.Status == PaymentStatus.processing),
            context.CancellationToken
        );

        if (!exists)
        {
            _db.OrderPayments.Add(
                new OrderPayment
                {
                    OrderId = order.OrderId,
                    Method = PaymentMethod.e_wallet,
                    Status = PaymentStatus.pending,
                    Amount = order.TotalAmount,
                    PaymentGateway = "zalopay",
                    RetryCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Order = order,
                }
            );
        }

        MarkProcessed(message.EventId, message.EventType, processedBy);
        await _db.SaveChangesAsync(context.CancellationToken);
    }

    public async Task Consume(ConsumeContext<InventoryReservationFailedEvent> context)
    {
        const string processedBy = "payment-consumer";
        var message = context.Message;

        if (await IsAlreadyProcessedAsync(message.EventId, processedBy, context.CancellationToken))
        {
            _logger.LogInformation(
                "PaymentConsumer skipped already processed event {EventId}",
                message.EventId
            );
            return;
        }

        var order = await _db.Orders.FirstOrDefaultAsync(
            o => o.OrderId == message.OrderId,
            context.CancellationToken
        );
        _db.OrderPayments.Add(
            new OrderPayment
            {
                OrderId = message.OrderId,
                Method = PaymentMethod.e_wallet,
                Status = PaymentStatus.failed,
                Amount = order?.TotalAmount ?? 0m,
                PaymentGateway = "zalopay",
                FailureReason = "Inventory reservation failed: " + message.Reason,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Order = order ?? null!,
            }
        );

        await PublishFailedAsync(
            orderId: message.OrderId,
            orderNumber: message.OrderNumber,
            userId: message.UserId,
            sourceEventId: message.EventId,
            reason: "Inventory reservation failed: " + message.Reason,
            errorCode: "inventory_reservation_failed",
            ct: context.CancellationToken
        );

        MarkProcessed(message.EventId, message.EventType, processedBy);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogWarning(
            "PaymentConsumer skipped gateway call due to inventory failure for order {OrderId}",
            message.OrderId
        );
    }

    private async Task<bool> IsAlreadyProcessedAsync(
        Guid eventId,
        string processedBy,
        System.Threading.CancellationToken ct
    )
    {
        return await _db.ProcessedEvents.AnyAsync(
            x => x.EventId == eventId && x.ProcessedBy == processedBy,
            ct
        );
    }

    private void MarkProcessed(Guid eventId, string eventType, string processedBy)
    {
        _db.ProcessedEvents.Add(
            new ProcessedEvent
            {
                EventId = eventId,
                EventType = eventType,
                ProcessedAt = DateTime.UtcNow,
                ProcessedBy = processedBy,
            }
        );
    }

    private async Task PublishFailedAsync(
        int orderId,
        string orderNumber,
        int userId,
        Guid sourceEventId,
        string reason,
        string? errorCode,
        System.Threading.CancellationToken ct
    )
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.OrderId == orderId, ct);
        var amount = order?.TotalAmount ?? 0m;

        await _publishEndpoint.Publish(
            new PaymentFailedEvent
            {
                OrderId = orderId,
                OrderNumber = orderNumber,
                UserId = userId,
                SourceEventId = sourceEventId,
                Reason = reason,
                ErrorCode = errorCode,
                Amount = amount,
            },
            ct
        );
    }
}
