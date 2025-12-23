using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public int? SkuId { get; set; }

    public required string ImageUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public required string AltText { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public required virtual Product Product { get; set; }

    public required virtual ProductSku Sku { get; set; }

    public static ProductImage CreateDefault(
        Product product,
        ProductSku sku,
        string imageUrl,
        string altText,
        bool isPrimary = true)
    {
        return new ProductImage
        {
            Product = product,
            Sku = sku,
            ImageUrl = imageUrl,
            ThumbnailUrl = imageUrl,
            AltText = altText,
            DisplayOrder = 1,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}