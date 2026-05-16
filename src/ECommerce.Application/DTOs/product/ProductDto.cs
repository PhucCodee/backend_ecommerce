using System;
using System.Collections.Generic;
using ECommerce.Application.DTOs.category;

namespace ECommerce.Application.DTOs.product
{
    public class ProductDto
    {
        public int Id { get; set; }
        public List<CategorySimpleDto> Categories { get; set; } = [];
        public int SellerId { get; set; }
        public string? SellerName { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? Slug { get; set; }
        public string? Brand { get; set; }
        public string? Sku { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal? WeightKg { get; set; }
        public string? DimensionsCm { get; set; }
        public string? Status { get; set; }
        public int ViewCount { get; set; }
        public int VariantCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
