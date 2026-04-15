using System;

namespace ECommerce.Application.DTOs.product
{
    public class ProductImageDto
    {
        public int Id { get; set; }
        public int ProductSkuId { get; set; }
        public required string ImageUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? AltText { get; set; }
        public bool IsPrimary { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}