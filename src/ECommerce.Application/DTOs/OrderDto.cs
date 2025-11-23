using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

public class OrderDto
{
    public int Id { get; set; } // Unique identifier for the order
    public int UserId { get; set; } // Identifier for the user who placed the order
    public DateTime OrderDate { get; set; } // Date when the order was placed
    public decimal TotalAmount { get; set; } // Total amount for the order
    public required OrderStatus Status { get; set; } = OrderStatus.created; // Current status of the order (e.g., Pending, Completed, Cancelled)
    public List<OrderItemDto>? OrderItems { get; set; } // List of items included in the order
}