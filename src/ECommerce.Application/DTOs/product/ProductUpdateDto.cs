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

        [Range(1, int.MaxValue)]
        public int? CategoryId { get; set; }

        [MaxLength(100)]
        public string? Brand { get; set; }

        public decimal? WeightKg { get; set; }

        [MaxLength(50)]
        public string? DimensionsCm { get; set; }

        [MaxLength(50)]
        public string? Status { get; set; }
    }
}