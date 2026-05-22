using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Abstractions.Events;

public interface IOrderEventService
{
    Task PublishOrderCreatedAsync(Order order);
    Task PublishOrderConfirmedAsync(Order order);
    Task PublishOrderShippedAsync(Order order);
    Task PublishOrderCancelledAsync(Order order, string? notes);

    Task PublishPaymentSucceededAsync(Order order, OrderPayment payment, string transactionId);
    Task PublishPaymentFailedAsync(
        Order order,
        OrderPayment payment,
        string reason,
        string? errorCode
    );
}
