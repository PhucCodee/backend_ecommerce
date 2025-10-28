using System;
using System.Collections.Generic;
using ECommerce.Domain.Common;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid UserId { get; set; } // Foreign key to the User entity
        public DateTime OrderDate { get; set; } // Date when the order was placed
        public OrderStatus Status { get; set; } // Current status of the order
        public decimal TotalAmount { get; set; } // Total amount for the order

        public virtual User User { get; set; } // Navigation property to the User entity
        public virtual ICollection<OrderItem> OrderItems { get; set; } // Collection of order items

        public Order()
        {
            OrderItems = new List<OrderItem>(); // Initialize the order items collection
        }
    }
}