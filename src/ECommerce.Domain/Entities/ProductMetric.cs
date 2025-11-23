using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class ProductMetric
{
    public int MetricId { get; set; }

    public int ProductId { get; set; }

    public DateOnly Date { get; set; }

    public int ViewCount { get; set; }

    public int ClickCount { get; set; }

    public int AddToCartCount { get; set; }

    public int PurchaseCount { get; set; }

    public decimal Revenue { get; set; }

    public decimal? AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public required virtual Product Product { get; set; }
}
