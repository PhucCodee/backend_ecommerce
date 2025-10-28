using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IOrderService
    {
        // Method to create a new order
        Task<OrderDto> CreateOrderAsync(OrderDto orderDto);

        // Method to retrieve an order by its ID
        Task<OrderDto> GetOrderByIdAsync(Guid orderId);

        // Method to retrieve all orders for a specific user
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(Guid userId);

        // Method to update an existing order
        Task<OrderDto> UpdateOrderAsync(OrderDto orderDto);

        // Method to delete an order by its ID
        Task<bool> DeleteOrderAsync(Guid orderId);
    }
}