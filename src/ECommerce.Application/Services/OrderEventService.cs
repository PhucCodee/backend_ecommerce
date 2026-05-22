using System;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Application.Abstractions.Events;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;

namespace ECommerce.Application.Services;

public class OrderEventService(IEventPublisher eventPublisher) : IOrderEventService
{
    private readonly IEventPublisher _eventPublisher = eventPublisher;

    public Task PublishOrderCreatedAsync(Order order) =>
        _eventPublisher.PublishAsync(BuildOrderCreatedEvent(order));

    public Task PublishOrderConfirmedAsync(Order order) =>
        _eventPublisher.PublishAsync(BuildOrderConfirmedEvent(order));

    public Task PublishOrderShippedAsync(Order order) =>
        _eventPublisher.PublishAsync(BuildOrderShippedEvent(order));

    public Task PublishOrderCancelledAsync(Order order, string? notes) =>
        _eventPublisher.PublishAsync(BuildOrderCancelledEvent(order, notes));

    public Task PublishPaymentSucceededAsync(
        Order order,
        OrderPayment payment,
        string transactionId
    ) => _eventPublisher.PublishAsync(BuildPaymentSucceededEvent(order, transactionId));

    public Task PublishPaymentFailedAsync(
        Order order,
        OrderPayment payment,
        string reason,
        string? errorCode
    ) => _eventPublisher.PublishAsync(BuildPaymentFailedEvent(order, reason, errorCode));

    private static OrderCreatedEvent BuildOrderCreatedEvent(Order order)
    {
        return new OrderCreatedEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            Subtotal = order.Subtotal,
            ShippingFee = order.ShippingFee,
            TaxAmount = order.TaxAmount,
            CouponCode = order.CouponCode,
            CouponDiscount = order.CouponDiscount,
            Items = order
                .OrderItems.Select(i => new OrderItemEvent
                {
                    SkuId = i.SkuId,
                    ProductName = i.ProductName,
                    Sku = i.Sku,
                    SellerId = i.SellerId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Subtotal = i.Subtotal,
                })
                .ToList(),
        };
    }

    private static OrderConfirmedEvent BuildOrderConfirmedEvent(Order order)
    {
        return new OrderConfirmedEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
        };
    }

    private static OrderShippedEvent BuildOrderShippedEvent(Order order)
    {
        return new OrderShippedEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
        };
    }

    private static OrderCancelledEvent BuildOrderCancelledEvent(Order order, string? notes)
    {
        return new OrderCancelledEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            Notes = notes,
        };
    }

    private static PaymentSucceededEvent BuildPaymentSucceededEvent(
        Order order,
        string transactionId
    )
    {
        return new PaymentSucceededEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            SourceEventId = Guid.Empty,
            TransactionId = transactionId,
            Amount = order.TotalAmount,
        };
    }

    private static PaymentFailedEvent BuildPaymentFailedEvent(
        Order order,
        string reason,
        string? errorCode
    )
    {
        return new PaymentFailedEvent
        {
            OrderId = order.OrderId,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            SourceEventId = Guid.Empty,
            Reason = reason,
            ErrorCode = errorCode,
            Amount = order.TotalAmount,
        };
    }
}
