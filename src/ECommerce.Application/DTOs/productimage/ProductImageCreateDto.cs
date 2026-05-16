using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.product
{
    public class ProductImageCreateDto
    {
        [Required(ErrorMessage = "Image URL is required")]
        [Url(ErrorMessage = "Invalid URL format")]
        [MaxLength(500)]
        public required string ImageUrl { get; set; }

        public string? ThumbnailUrl { get; set; }

        [MaxLength(255)]
        public string? AltText { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
