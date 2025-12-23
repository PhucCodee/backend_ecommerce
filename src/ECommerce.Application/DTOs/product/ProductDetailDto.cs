using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.product
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int SellerId { get; set; }
        public string? SellerName { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public string? Brand { get; set; }
        public string? Sku { get; set; } // SKU code
        public bool IsDefault { get; set; } // True = primary product, False = variant
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; } // Primary image for backward compatibility
        public List<ProductImageDto>? Images { get; set; } // All images
        public decimal? WeightKg { get; set; }
        public string? DimensionsCm { get; set; }
        public string? Status { get; set; }
        public int ViewCount { get; set; }
        public int VariantCount { get; set; } // Number of variants for this product
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}