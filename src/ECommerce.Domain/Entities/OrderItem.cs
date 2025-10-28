using System;

namespace ECommerce.Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; } // Unique identifier for the order item
        public Guid OrderId { get; set; } // Foreign key to the associated order
        public Guid ProductId { get; set; } // Foreign key to the associated product
        public int Quantity { get; set; } // Quantity of the product in the order
        public decimal Price { get; set; } // Price of the product at the time of order

        // Navigation properties
        public virtual Order Order { get; set; } // Navigation property to the Order
        public virtual Product Product { get; set; } // Navigation property to the Product
    }
}