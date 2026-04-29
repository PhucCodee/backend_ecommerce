using System;

namespace ECommerce.Domain.Events;

public record PaymentSucceededEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "payment.succeeded";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required int OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required int UserId { get; init; }
    public required Guid SourceEventId { get; init; }
    public required string TransactionId { get; init; }
    public required decimal Amount { get; init; }
}
