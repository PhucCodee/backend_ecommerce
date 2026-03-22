using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.category
{
    public class CategoryUpdateDto
    {
        [StringLength(100, MinimumLength = 2)]
        public string? Name { get; set; }

        public int? ParentCategoryId { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public int? DisplayOrder { get; set; }

        public bool? IsCore { get; set; }

        public bool? IsActive { get; set; }
    }
}