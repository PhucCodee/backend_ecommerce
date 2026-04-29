using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories;

public interface IOrderPaymentRepository : IRepository<OrderPayment>
{
    Task<OrderPayment?> GetLatestByOrderIdAndGatewayAsync(int orderId, string gateway);
    Task<bool> HasCompletedPaymentAsync(int orderId);
}
