using System.Collections.Generic;

namespace ECommerce.Application.DTOs.product
{
    public class ProductImageDto
    {
        public required string ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? AltText { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class ProductCreateDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public int CategoryId { get; set; }
        public int SellerId { get; set; }
        public string? ImageUrl { get; set; } // Primary image (backward compatible)
        public List<ProductImageDto>? Images { get; set; } // Multiple images
        public string? Brand { get; set; }
        public decimal? WeightKg { get; set; }
        public string? DimensionsCm { get; set; }
    }
}