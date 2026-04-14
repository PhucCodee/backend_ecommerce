using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.DTOs.order;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    private readonly IOrderService _orderService = orderService;

    // Create a new order from the current cart
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = GetCurrentUserId();
        var order = await _orderService.CreateAsync(userId, request);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<OrderDto>.Ok(order, "Order created successfully"));
    }

    // Get a specific order by ID
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(int orderId)
    {
        var userId = GetCurrentUserId();
        var order = await _orderService.GetByIdAsync(userId, orderId);

        if (order == null)
            return NotFound(ApiResponse<object>.Fail("Order not found"));

        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    // Get all orders for the current user
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] PaginationParams paginationParams)
    {
        var userId = GetCurrentUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId, paginationParams);
        return Ok(ApiResponse<PagedResult<OrderSummaryDto>>.Ok(orders));
    }

    // Cancel an order
    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var userId = GetCurrentUserId();
        var order = await _orderService.CancelAsync(userId, orderId);

        if (order == null)
            return NotFound(ApiResponse<object>.Fail("Order not found"));

        return Ok(ApiResponse<OrderDto>.Ok(order, "Order cancelled successfully"));
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedException("Invalid user token");
        return userId;
    }
}
