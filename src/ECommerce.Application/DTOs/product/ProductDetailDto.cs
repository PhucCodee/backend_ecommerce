using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.product
{
    public class ProductDetailDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string? CategoryName { get; set; }
        public List<int> CategoryIds { get; set; } = [];
        public List<string> CategoryNames { get; set; } = [];
        public string? SellerName { get; set; }
        public decimal? WeightKg { get; set; }
        public string? DimensionsCm { get; set; }
        public int VariantCount { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public bool InStock { get; set; }
        public List<ProductImageDto> Images { get; set; } = [];
        public List<ProductDetailSkuDto> Skus { get; set; } = [];
    }
}
