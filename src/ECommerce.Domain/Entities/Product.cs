using System;
using ECommerce.Domain.Common;

namespace ECommerce.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } // The name of the product
        public string Description { get; set; } // A brief description of the product
        public decimal Price { get; set; } // The price of the product
        public int Stock { get; set; } // The available stock quantity of the product
        public string ImageUrl { get; set; } // The URL of the product image
        public string Category { get; set; } // The category of the product
    }
}