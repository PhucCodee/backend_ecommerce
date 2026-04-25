using ECommerce.Domain.Entities;
using ECommerce.Domain.Events;
using ECommerce.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Worker;

public sealed class PaymentConsumer :
    IConsumer<InventoryReservedEvent>,
    IConsumer<InventoryReservationFailedEvent>
{
    // inject ApplicationDbContext, IPaymentGatewayClient, IPublishEndpoint, ILogger

    public async Task Consume(ConsumeContext<InventoryReservedEvent> context)
    {
        // idempotency check (eventId + processedBy)
        // create payment row status=processing
        // call gateway client
        // update payment row to completed/failed
        // publish PaymentSucceededEvent or PaymentFailedEvent
        // mark ProcessedEvents
        // save changes
    }

    public async Task Consume(ConsumeContext<InventoryReservationFailedEvent> context)
    {
        // no gateway call
        // create failed payment row or order note
        // publish PaymentFailedEvent with reason "inventory reservation failed"
        // mark processed and save
    }
}