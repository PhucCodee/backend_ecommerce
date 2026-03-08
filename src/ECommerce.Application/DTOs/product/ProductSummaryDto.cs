namespace ECommerce.Application.DTOs.product
{
    public class ProductSummaryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Slug { get; set; }
        public string? Brand { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }           // From default SKU
        public decimal? CompareAtPrice { get; set; } // For "was $X, now $Y" display
        public bool InStock { get; set; }            // Simple boolean instead of count
        public string? ThumbnailUrl { get; set; }    // First image of default SKU
        public int VariantCount { get; set; }        // "Available in 5 variants"
    }
}