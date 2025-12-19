using System;

namespace ECommerce.Domain.Entities;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int CartId { get; set; }

    public int SkuId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceSnapshot { get; set; }

    public DateTime AddedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public required virtual ProductSku Sku { get; set; }

    public required virtual Cart Cart { get; set; }
}
