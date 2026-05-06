using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class ProductSku
{
    public int SkuId { get; set; }

    public int ProductId { get; set; }

    public required string Sku { get; set; } = string.Empty;

    public string? VariantAttributes { get; set; }

    public decimal Price { get; set; }

    public decimal? CostPrice { get; set; }

    public decimal? CompareAtPrice { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDefault { get; set; } = false;

    public decimal? WeightKg { get; set; }

    public string? DimensionsCm { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = [];

    public virtual ICollection<OrderItem> OrderItems { get; set; } = [];

    public virtual ICollection<ProductImage> ProductImages { get; set; } = [];

    public virtual Inventory? Inventory { get; set; }

    public virtual required Product Product { get; set; }

    public static ProductSku CreateDefault(Product product, string sku, decimal price)
    {
        return new ProductSku
        {
            Product = product,
            Sku = sku,
            VariantAttributes = "{}",
            Price = price,
            IsActive = true,
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
