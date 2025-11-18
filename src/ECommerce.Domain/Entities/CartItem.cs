using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int UserId { get; set; }

    public int SkuId { get; set; }

    public int Quantity { get; set; }

    public decimal PriceSnapshot { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ProductSku Sku { get; set; }

    public virtual User User { get; set; }
}
