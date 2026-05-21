namespace ECommerce.Application.DTOs.product;

public class ProductDetailSkuDto
{
    public string Sku { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int Stock { get; set; }
}
