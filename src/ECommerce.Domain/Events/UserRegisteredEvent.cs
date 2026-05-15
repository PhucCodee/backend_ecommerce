using System;

namespace ECommerce.Domain.Events;

public record UserRegisteredEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType => "user.registered";
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required int UserId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}
