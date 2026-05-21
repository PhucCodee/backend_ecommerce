using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.order;

namespace ECommerce.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateAsync(int userId, CreateOrderRequest request);
    Task<OrderDto?> GetByIdAsync(int userId, int orderId);
    Task<PagedResult<OrderSummaryDto>> GetUserOrdersAsync(
        int userId,
        PaginationParams paginationParams
    );
    Task<OrderDto?> CancelAsync(int userId, int orderId);
    Task<PagedResult<OrderSummaryDto>> GetAllOrdersAsync(PaginationParams paginationParams);
    Task<OrderDto?> GetByIdAsAdminAsync(int orderId);
    Task<PagedResult<OrderSummaryDto>> GetSellerOrdersAsync(
        int sellerId,
        PaginationParams paginationParams
    );
    Task<OrderDto?> GetSellerOrderByIdAsync(int sellerId, int orderId);
    Task<OrderDto?> UpdateStatusAsSellerAsync(
        int sellerId,
        int orderId,
        UpdateOrderStatusRequest request
    );
}
