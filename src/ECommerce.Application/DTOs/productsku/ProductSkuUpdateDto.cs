namespace ECommerce.Application.DTOs.productsku
{
    public class ProductSkuUpdateDto
    {
        public string? VariantAttributes { get; set; }
        public decimal? Price { get; set; }
        public decimal? CostPrice { get; set; }
        public decimal? CompareAtPrice { get; set; }
        public int? Stock { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDefault { get; set; }
        public decimal? WeightKg { get; set; }
        public string? DimensionsCm { get; set; }
        public string? ImageUrl { get; set; }
    }
}