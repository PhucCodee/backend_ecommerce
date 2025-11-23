using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class Category
{
    public int CategoryId { get; set; }

    public required string CategoryName { get; set; }

    public required string Slug { get; set; }

    public int? ParentCategoryId { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Category> InverseParentCategory { get; set; } = [];

    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Product> Products { get; set; } = [];
}
