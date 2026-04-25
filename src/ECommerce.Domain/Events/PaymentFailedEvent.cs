using System;

public record PaymentFailedEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "payment.failed";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required int OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required int UserId { get; init; }
    public required Guid SourceEventId { get; init; }
    public required string Reason { get; init; }
    public string? ErrorCode { get; init; }
}