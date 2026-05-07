using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderPaymentRepository(ApplicationDbContext context)
    : Repository<OrderPayment>(context),
        IOrderPaymentRepository
{
    public async Task<OrderPayment?> GetLatestByOrderIdAndGatewayAsync(int orderId, string gateway)
    {
        return await _context
            .OrderPayments.Where(p => p.OrderId == orderId && p.PaymentGateway == gateway)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasCompletedPaymentAsync(int orderId)
    {
        return await _context.OrderPayments.AnyAsync(p =>
            p.OrderId == orderId && p.Status == PaymentStatus.completed
        );
    }
}
