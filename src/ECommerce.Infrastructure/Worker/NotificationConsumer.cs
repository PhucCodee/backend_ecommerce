using System.Threading.Tasks;
using ECommerce.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Worker;

public sealed class NotificationConsumer(ILogger<NotificationConsumer> logger)
    : IConsumer<PaymentSucceededEvent>,
        IConsumer<PaymentFailedEvent>
{
    private readonly ILogger<NotificationConsumer> _logger = logger;

    public Task Consume(ConsumeContext<PaymentSucceededEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation(
            "NotificationConsumer received payment.succeeded {EventId} for order {OrderId} user {UserId}",
            message.EventId,
            message.OrderId,
            message.UserId
        );

        return Task.CompletedTask;
    }

    public Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogWarning(
            "NotificationConsumer received payment.failed {EventId} for order {OrderId} user {UserId}. Reason={Reason}",
            message.EventId,
            message.OrderId,
            message.UserId,
            message.Reason
        );

        return Task.CompletedTask;
    }
}
