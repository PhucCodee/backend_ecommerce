using System.Collections.Generic;
namespace ECommerce.Application.DTOs.product
{
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Brand { get; set; }
        public int SellerId { get; set; }
        public string? SellerName { get; set; }
        public string? PrimaryCategoryName { get; set; }
        public List<string> CategoryNames { get; set; } = [];
        public List<int> CategoryIds { get; set; } = [];
        public decimal Price { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public bool InStock { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int VariantCount { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public string Status { get; set; } = "active";
        public bool IsSuspended { get; set; } = false;
    }
}