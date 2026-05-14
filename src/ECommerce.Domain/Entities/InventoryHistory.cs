using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class InventoryHistory
{
    public int HistoryId { get; set; }

    public int InventoryId { get; set; }

    public string? ChangeType { get; set; }

    public int QuantityChange { get; set; }

    public int QuantityBefore { get; set; }

    public int QuantityAfter { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? Notes { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual required User ChangedByNavigation { get; set; }

    public virtual required Inventory Inventory { get; set; }
}
