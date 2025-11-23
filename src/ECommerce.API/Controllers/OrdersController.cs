using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {
        private readonly IOrderService _orderService = orderService;

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            // Placeholder: return a list with one dummy order
            var orders = await Task.FromResult(new List<OrderDto>
            {
                new OrderDto
                {
                    Id = 0,
                    UserId = 0,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = 100,
                    Status = Domain.Enums.OrderStatus.created,
                    OrderItems = []
                }
            });
            return Ok(orders);
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
        {
            // Placeholder: return a dummy order if id is not empty
            if (id == Guid.Empty)
                return NotFound();

            var order = await Task.FromResult(new OrderDto
            {
                Id = 0,
                UserId = 0,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 100,
                Status = Domain.Enums.OrderStatus.created,
                OrderItems = []
            });
            return Ok(order);
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(OrderDto orderDto)
        {
            // Placeholder: echo back the posted order with a new Id
            orderDto.Id = 0;
            var createdOrder = await Task.FromResult(orderDto);
            return CreatedAtAction(nameof(GetOrder), new { id = createdOrder.Id }, createdOrder);
        }
    }
}