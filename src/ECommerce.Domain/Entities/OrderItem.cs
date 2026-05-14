using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int SkuId { get; set; }

    public required string ProductName { get; set; }

    public required string Sku { get; set; }

    public string? VariantDescription { get; set; }

    public int SellerId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual required Order Order { get; set; }

    public virtual required User Seller { get; set; }

    public virtual required ProductSku SkuNavigation { get; set; }

    public virtual ICollection<Review> Reviews { get; set; } = [];

    public static OrderItem CreateDefault(
        Order order,
        int skuId,
        string productName,
        string sku,
        int sellerId,
        int quantity,
        decimal unitPrice,
        string? variantDescription = null
    )
    {
        return new OrderItem
        {
            OrderId = order.OrderId,
            SkuId = skuId,
            ProductName = productName,
            Sku = sku,
            VariantDescription = variantDescription,
            SellerId = sellerId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Subtotal = unitPrice * quantity,
            CreatedAt = DateTime.UtcNow,
            Order = order,
            Seller = null!,
            SkuNavigation = null!,
        };
    }
}
