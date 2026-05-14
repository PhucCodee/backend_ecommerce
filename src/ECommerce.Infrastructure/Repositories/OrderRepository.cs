using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class OrderRepository(ApplicationDbContext context)
        : Repository<Order>(context),
            IOrderRepository
    {
        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
        {
            return await _context
                .Orders.Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.SkuNavigation)
                        .ThenInclude(s => s.ProductImages)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.SkuNavigation)
                        .ThenInclude(s => s.Product)
                            .ThenInclude(p => p.ProductSkus)
                                .ThenInclude(ps => ps.ProductImages)
                .Include(o => o.OrderShipping)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize
        )
        {
            // Include OrderItems so the OrderSummaryDto.TotalItems mapping
            // (Sum of OrderItems.Quantity) returns the real count instead of 0.
            var query = _context.Orders.Include(o => o.OrderItems).Where(o => o.UserId == userId);

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersPagedAsync(
            int pageNumber,
            int pageSize
        )
        {
            var query = _context.Orders.Include(o => o.OrderItems);

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }

        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersBySellerIdAsync(
            int sellerId,
            int pageNumber,
            int pageSize
        )
        {
            var query = _context
                .Orders.Include(o => o.OrderItems)
                .Where(o => o.OrderItems.Any(i => i.SellerId == sellerId));

            var totalCount = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (orders, totalCount);
        }
    }
}
