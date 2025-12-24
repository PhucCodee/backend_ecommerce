namespace ECommerce.Application.DTOs.productsku
{
    public class ProductSkuCreateDto
    {
        public int ProductId { get; set; }
        public required string VariantAttributes { get; set; } = "{}";
        public decimal Price { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int Stock { get; set; }
        public bool IsDefault { get; set; } = false;
        public decimal? WeightKg { get; set; }
        public string? DimensionsCm { get; set; }
        public string? ImageUrl { get; set; }
    }
}