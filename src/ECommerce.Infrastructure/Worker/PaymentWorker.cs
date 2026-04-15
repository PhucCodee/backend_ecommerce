using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Infrastructure.Worker;

public sealed class PaymentWorker(
    IConnection connection,
    IServiceProvider serviceProvider,
    ILogger<PaymentWorker> logger) : BackgroundService
{
    private const string ExchangeName = "order.events";
    private const string QueueName = "order.created.payment";
    private const string RoutingKey = "order.created";

    private readonly IConnection _connection = connection;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<PaymentWorker> _logger = logger;

    private IModel? _channel;

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true);

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey);

        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        _logger.LogInformation("PaymentWorker started and listening on queue {QueueName}", QueueName);
        return base.StartAsync(cancellationToken);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_channel is null)
        {
            throw new InvalidOperationException("Channel was not initialized.");
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
                var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(payload);

                if (orderCreated is null)
                {
                    throw new InvalidOperationException("Invalid event payload for order.created.");
                }

                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Idempotency: skip if already processed by payment worker.
                var alreadyProcessed = await db.ProcessedEvents.AnyAsync(
                    x => x.EventId == orderCreated.EventId && x.ProcessedBy == "payment-worker",
                    stoppingToken);

                if (alreadyProcessed)
                {
                    _channel.BasicAck(ea.DeliveryTag, multiple: false);
                    return;
                }

                // TODO: Implement payment creation/charge logic here.
                // Example: create OrderPayment row with pending/succeeded status.

                db.ProcessedEvents.Add(new ProcessedEvent
                {
                    EventId = orderCreated.EventId,
                    EventType = orderCreated.EventType,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = "payment-worker"
                });

                await db.SaveChangesAsync(stoppingToken);

                _channel.BasicAck(ea.DeliveryTag, multiple: false);

                _logger.LogInformation(
                    "PaymentWorker processed event {EventId} for order {OrderId}",
                    orderCreated.EventId,
                    orderCreated.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PaymentWorker failed to process order.created event");
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel is not null)
        {
            _channel.Close();
            _channel.Dispose();
        }

        return base.StopAsync(cancellationToken);
    }
}