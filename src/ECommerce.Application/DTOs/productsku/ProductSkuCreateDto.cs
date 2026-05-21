using System.Collections.Generic;
using ECommerce.Application.DTOs.inventory;
using ECommerce.Application.DTOs.product;

namespace ECommerce.Application.DTOs.product;

public class ProductSkuCreateDto
{
    public int ProductId { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? WeightKg { get; set; }
    public string? DimensionsCm { get; set; }
    public int Stock { get; set; }
    public List<ProductImageCreateDto>? Images { get; set; }
    public InventoryCreateDto? Inventory { get; set; }
}
