using System;

namespace ECommerce.Application.DTOs.cart
{
    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int SkuId { get; set; }
        public int SellerId { get; set; }
        public string? Sku { get; set; }
        public string? ProductName { get; set; }
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal PriceSnapshot { get; set; }
        public decimal CurrentPrice { get; set; }
        public int AvailableStock { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
