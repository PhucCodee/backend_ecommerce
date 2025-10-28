using System;
using System.Collections.Generic;

public class OrderDto
{
    public Guid Id { get; set; } // Unique identifier for the order
    public Guid UserId { get; set; } // Identifier for the user who placed the order
    public DateTime OrderDate { get; set; } // Date when the order was placed
    public decimal TotalAmount { get; set; } // Total amount for the order
    public string Status { get; set; } // Current status of the order (e.g., Pending, Completed, Cancelled)
    public List<OrderItemDto> OrderItems { get; set; } // List of items included in the order
}