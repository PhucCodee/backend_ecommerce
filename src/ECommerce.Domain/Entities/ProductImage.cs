using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class ProductImage
{
    public int ImageId { get; set; }

    public int ProductId { get; set; }

    public int? SkuId { get; set; }

    public string ImageUrl { get; set; }

    public string ThumbnailUrl { get; set; }

    public string AltText { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Product Product { get; set; }

    public virtual ProductSku Sku { get; set; }
}
