using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ECommerce.Application.DTOs.inventory;

namespace ECommerce.Application.DTOs.product
{
    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MinLength(3, ErrorMessage = "Product name must be at least 3 characters")]
        [MaxLength(255, ErrorMessage = "Product name cannot exceed 255 characters")]
        public required string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one category is required")]
        public List<int> CategoryIds { get; set; } = [];

        [MaxLength(100)]
        public string? Brand { get; set; }

        public decimal? WeightKg { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal DefaultSkuPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int DefaultSkuStock { get; set; }

        [MaxLength(50)]
        public string? DimensionsCm { get; set; }

        public InventoryCreateDto? DefaultSkuInventory { get; set; }
    }
}
