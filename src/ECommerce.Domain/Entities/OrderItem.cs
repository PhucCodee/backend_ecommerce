using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class OrderItem
{
    public int OrderItemId { get; set; }

    public int OrderId { get; set; }

    public int SkuId { get; set; }

    public string ProductName { get; set; }

    public string Sku { get; set; }

    public string VariantDescription { get; set; }

    public int SellerId { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal Subtotal { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; }

    public virtual User Seller { get; set; }

    public virtual ProductSku SkuNavigation { get; set; }
}
