using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.product
{
    public class ProductImageUpdateDto
    {
        public int? Id { get; set; }

        [Url(ErrorMessage = "Invalid URL format")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public string? ThumbnailUrl { get; set; }

        [MaxLength(255)]
        public string? AltText { get; set; }

        public bool? IsPrimary { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsDeleted { get; set; }
    }
}
