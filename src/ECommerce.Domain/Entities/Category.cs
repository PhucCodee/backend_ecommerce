using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public int? ParentCategoryId { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsCore { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Category> ChildCategories { get; set; } = [];

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = [];

    public static Category CreateDefault(
        string name,
        string slug,
        int? parentCategoryId = null,
        string? description = null,
        string? imageUrl = null,
        int displayOrder = 0,
        bool isCore = true,
        bool isActive = true
    )
    {
        return new Category
        {
            CategoryName = name,
            Slug = slug,
            ParentCategoryId = parentCategoryId,
            Description = description,
            ImageUrl = imageUrl,
            DisplayOrder = displayOrder,
            IsCore = isCore,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void SoftDelete()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsDeleted() => !IsActive;
}
