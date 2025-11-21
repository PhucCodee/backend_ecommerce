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

    public required virtual Order Order { get; set; }

    public required virtual User Seller { get; set; }

    public required virtual ProductSku SkuNavigation { get; set; }
}
