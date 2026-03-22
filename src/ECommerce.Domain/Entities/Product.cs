using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public int SellerId { get; set; }

    public required string ProductName { get; set; } = string.Empty;

    public required string Slug { get; set; } = string.Empty;

    public required string BaseSku { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? ShortDescription { get; set; }

    public bool HasVariants { get; set; } = false;

    public string? Brand { get; set; }

    public decimal? WeightKg { get; set; }

    public string? DimensionsCm { get; set; }

    public ProductStatus Status { get; set; } = ProductStatus.draft;

    public ModerationStatus Moderation { get; set; } = ModerationStatus.pending;

    public string? MetaTitle { get; set; }

    public string? MetaDescription { get; set; }

    public string? Tags { get; set; }

    public int ViewCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? RemovedAt { get; set; }

    public virtual ICollection<ProductCategory> ProductCategories { get; set; } = [];

    public virtual ICollection<ProductMetric> ProductMetrics { get; set; } = [];

    public virtual ICollection<ProductSku> ProductSkus { get; set; } = [];

    public virtual ICollection<Review> Reviews { get; set; } = [];

    public virtual ICollection<UserItemInteraction> UserItemInteractions { get; set; } = [];

    public required virtual User Seller { get; set; }

    public static Product CreateDefault(
        string name,
        string slug,
        string baseSku,
        int sellerId,
        string? description = null,
        string? brand = null,
        decimal? weightKg = null,
        string? dimensionsCm = null)
    {
        return new Product
        {
            ProductName = name,
            Slug = slug,
            BaseSku = baseSku,
            SellerId = sellerId,
            Description = description,
            Brand = brand,
            WeightKg = weightKg,
            DimensionsCm = dimensionsCm,
            HasVariants = false,
            Status = ProductStatus.active,
            Moderation = ModerationStatus.approved,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Seller = null!
        };
    }

    public void SoftDelete()
    {
        RemovedAt = DateTime.UtcNow;
        Status = ProductStatus.removed;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsDeleted() => RemovedAt.HasValue;
}