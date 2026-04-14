using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.order;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(int userId, CreateOrderRequest request);
    Task<OrderDto?> GetByIdAsync(int userId, int orderId);
    Task<PagedResult<OrderSummaryDto>> GetUserOrdersAsync(int userId, PaginationParams paginationParams);
    Task<OrderDto?> CancelAsync(int userId, int orderId);
}
