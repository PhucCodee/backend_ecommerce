using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public int SellerId { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; }

    public string Slug { get; set; }

    public string BaseSku { get; set; }

    public string Description { get; set; }

    public string ShortDescription { get; set; }

    public bool HasVariants { get; set; }

    public string Brand { get; set; }

    public decimal? WeightKg { get; set; }

    public string DimensionsCm { get; set; }

    public ProductStatus Status { get; set; }

    public ModerationStatus Moderation { get; set; }

    public string MetaTitle { get; set; }

    public string MetaDescription { get; set; }

    public string Tags { get; set; }

    public int ViewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime? RemovedAt { get; set; }

    public virtual Category Category { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductMetric> ProductMetrics { get; set; } = new List<ProductMetric>();

    public virtual ICollection<ProductSku> ProductSkus { get; set; } = new List<ProductSku>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual User Seller { get; set; }

    public virtual ICollection<UserItemInteraction> UserItemInteractions { get; set; } = new List<UserItemInteraction>();
}
