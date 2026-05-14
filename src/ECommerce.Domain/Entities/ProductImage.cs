using System;

namespace ECommerce.Domain.Entities;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int SkuId { get; set; }

    public required string ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public required string AltText { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime UpdatedAt { get; set; }

    public virtual required ProductSku Sku { get; set; }

    public static ProductImage CreateDefault(
        ProductSku sku,
        string imageUrl,
        string altText,
        bool isPrimary = true
    )
    {
        return new ProductImage
        {
            Sku = sku,
            ImageUrl = imageUrl,
            ThumbnailUrl = imageUrl,
            AltText = altText,
            DisplayOrder = 1,
            IsPrimary = isPrimary,
            IsDeleted = false,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Restore()
    {
        IsDeleted = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
