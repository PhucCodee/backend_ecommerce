using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(
            int userId,
            int pageNumber,
            int pageSize
        );
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersPagedAsync(
            int pageNumber,
            int pageSize
        );
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersBySellerIdAsync(
            int sellerId,
            int pageNumber,
            int pageSize
        );
    }
}
