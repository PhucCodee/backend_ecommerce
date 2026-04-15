using System;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class EventPublisher(
    ApplicationDbContext context,
    ILogger<EventPublisher> logger) : IEventPublisher
{
    private readonly ApplicationDbContext _context = context;
    private readonly ILogger<EventPublisher> _logger = logger;

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var eventId = GetEventId(@event);
        var eventType = GetEventType(@event);
        var orderId = GetOrderId(@event);
        var startedAt = DateTime.UtcNow;

        var eventLog = new EventLog
        {
            EventId = eventId,
            EventType = eventType,
            AttemptNumber = 1,
            Status = EventStatus.pending,
            WorkerName = "order-service",
            OrderId = orderId,
            Payload = JsonSerializer.Serialize(@event),
            StartedAt = startedAt
        };

        _context.EventLogs.Add(eventLog);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Event published: {EventType} with ID {EventId} for Order {OrderId}",
            eventType, eventId, orderId);
    }

    private static Guid GetEventId<TEvent>(TEvent @event)
    {
        var property = typeof(TEvent).GetProperty("EventId");
        if (property != null && property.PropertyType == typeof(Guid))
        {
            return (Guid)property.GetValue(@event)!;
        }
        return Guid.NewGuid();
    }

    private static string GetEventType<TEvent>(TEvent @event)
    {
        var property = typeof(TEvent).GetProperty("EventType");
        if (property != null && property.PropertyType == typeof(string))
        {
            return (string)property.GetValue(@event)!;
        }
        return typeof(TEvent).Name;
    }

    private static int? GetOrderId<TEvent>(TEvent @event)
    {
        var property = typeof(TEvent).GetProperty("OrderId");
        if (property != null && property.PropertyType == typeof(int))
        {
            return (int)property.GetValue(@event)!;
        }
        return null;
    }
}
