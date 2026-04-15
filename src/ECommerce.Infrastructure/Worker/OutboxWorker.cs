using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using ECommerce.Infrastructure.Data;
using ECommerce.Domain.Enums;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace ECommerce.Infrastructure.Worker;

public sealed class OutboxWorker(
    IServiceProvider serviceProvider,
    ILogger<OutboxWorker> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<OutboxWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IModel>();

                // 1) Pull a small batch
                var pending = await db.EventLogs
                    .Where(e => e.Status == EventStatus.pending)
                    .OrderBy(e => e.StartedAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var evt in pending)
                {
                    try
                    {
                        // 2) Publish to RabbitMQ
                        var body = Encoding.UTF8.GetBytes(evt.Payload);
                        publisher.BasicPublish(
                            exchange: "order.events",
                            routingKey: evt.EventType,
                            basicProperties: null,
                            body: body);

                        // 3) Mark completed
                        evt.Status = EventStatus.success;
                    }
                    catch (Exception ex)
                    {
                        evt.Status = EventStatus.failed;
                        evt.AttemptNumber += 1;
                        _logger.LogError(ex, "Outbox publish failed for event {EventId}", evt.EventId);
                    }
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox worker loop failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}