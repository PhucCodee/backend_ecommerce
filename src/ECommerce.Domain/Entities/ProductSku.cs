using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class ProductSku
{
    public int SkuId { get; set; }

    public int ProductId { get; set; }

    public string Sku { get; set; }

    public string VariantAttributes { get; set; }

    public decimal Price { get; set; }

    public decimal? CostPrice { get; set; }

    public decimal? CompareAtPrice { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public decimal? WeightKg { get; set; }

    public string DimensionsCm { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Inventory Inventory { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Product Product { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
