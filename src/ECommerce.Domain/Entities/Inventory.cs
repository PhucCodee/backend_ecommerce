using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class Inventory
{
    public int InventoryId { get; set; }

    public int SkuId { get; set; }

    public int QuantityAvailable { get; set; }

    public int QuantityReserved { get; set; }

    public int QuantitySold { get; set; }

    public int ReorderPoint { get; set; }

    public int ReorderQuantity { get; set; }

    public DateTime? LastRestockedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<InventoryHistory> InventoryHistories { get; set; } = [];

    public required virtual ProductSku Sku { get; set; }
}
