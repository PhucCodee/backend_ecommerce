using ECommerce.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Worker;

public sealed class NotificationConsumer(ILogger<NotificationConsumer> logger) : IConsumer<OrderCreatedEvent>
{
    private readonly ILogger<NotificationConsumer> _logger = logger;

    public Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "NotificationConsumer received order.created {EventId} for order {OrderId} for user {UserId}",
            message.EventId,
            message.OrderId,
            message.UserId);

        // TODO: send email/SMS/push notification
        return Task.CompletedTask;
    }
}