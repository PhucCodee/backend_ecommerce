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

    public static CartItem CreateDefault(Cart cart, ProductSku sku, int quantity)
    {
        return new CartItem
        {
            Cart = cart,
            CartId = cart.CartId,
            Sku = sku,
            SkuId = sku.SkuId,
            Quantity = quantity,
            PriceSnapshot = sku.Price,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}