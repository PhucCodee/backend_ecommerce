using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.product;

public class ProductSkuDto
{
    public int SkuId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public decimal? WeightKg { get; set; }
    public string? DimensionsCm { get; set; }
    public int Stock { get; set; }
    public int QuantityReserved { get; set; }
    public int QuantitySold { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQuantity { get; set; }
    public DateTime? LastRestockedAt { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
