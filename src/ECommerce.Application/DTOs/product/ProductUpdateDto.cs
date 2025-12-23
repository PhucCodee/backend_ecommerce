namespace ECommerce.Application.DTOs.product
{
    public class ProductUpdateDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public int? CategoryId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public decimal? WeightKg { get; set; }
        public string? DimensionsCm { get; set; }
        public string? Status { get; set; }
    }
}