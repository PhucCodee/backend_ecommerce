using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.order;

public class OrderDto
{
    public int OrderId { get; set; }
    public required string OrderNumber { get; set; }
    public int UserId { get; set; }
    public required string Status { get; set; }
    public required string PaymentStatus { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal TaxAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal CouponDiscount { get; set; }
    public decimal TotalAmount { get; set; }
    public required string Currency { get; set; }
    public string? CustomerNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int OrderItemId { get; set; }
    public int SkuId { get; set; }
    public int ProductId { get; set; }
    public int SellerId { get; set; }
    public string? SellerName { get; set; }
    public required string ProductName { get; set; }
    public required string Sku { get; set; }
    public string? VariantDescription { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
