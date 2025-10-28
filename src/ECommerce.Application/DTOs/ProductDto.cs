using System;

namespace ECommerce.Application.DTOs
{
    public class ProductDto
    {
        public Guid Id { get; set; } // Unique identifier for the product
        public string Name { get; set; } // Name of the product
        public string Description { get; set; } // Description of the product
        public decimal Price { get; set; } // Price of the product
        public int Stock { get; set; } // Available stock for the product
        public string ImageUrl { get; set; } // URL of the product image
    }
}