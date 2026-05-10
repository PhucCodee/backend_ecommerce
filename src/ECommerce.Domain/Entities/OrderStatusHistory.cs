using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class OrderStatusHistory
{
    public int HistoryId { get; set; }

    public int OrderId { get; set; }

    public OrderStatus OldStatus { get; set; }

    public OrderStatus NewStatus { get; set; }

    public string? Notes { get; set; }

    public int? ChangedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual required User ChangedByNavigation { get; set; }

    public virtual required Order Order { get; set; }
}
