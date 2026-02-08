using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.product
{
    public class ProductUpdateDto
    {
        [MinLength(3)]
        [MaxLength(255)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Slug { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int? Stock { get; set; }

        [Range(1, int.MaxValue)]
        public int? CategoryId { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        public decimal? WeightKg { get; set; }

        [MaxLength(50)]
        public string? DimensionsCm { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }

        [Url]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public List<ProductImageUpdateDto>? Images { get; set; }
    }
}