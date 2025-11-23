using System;

public class OrderItemDto
{
    public int Id { get; set; } // Unique identifier for the order item
    public int OrderId { get; set; } // Identifier for the associated order
    public int ProductId { get; set; } // Identifier for the associated product
    public int Quantity { get; set; } // Quantity of the product in the order
    public decimal Price { get; set; } // Price of the product at the time of order
}