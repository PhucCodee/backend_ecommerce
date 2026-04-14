using ECommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
        Task<(IEnumerable<Order> Orders, int TotalCount)> GetOrdersByUserIdAsync(int userId, int pageNumber, int pageSize);
    }
}