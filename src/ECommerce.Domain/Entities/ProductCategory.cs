namespace ECommerce.Domain.Entities;

public class ProductCategory
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public bool IsPrimary { get; set; }
    public virtual Product Product { get; set; } = null!;
    public virtual Category Category { get; set; } = null!;
}
