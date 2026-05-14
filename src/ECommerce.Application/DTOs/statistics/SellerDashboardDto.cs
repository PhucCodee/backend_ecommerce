using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.statistics
{
    public class SellerDashboardDto
    {
        public SellerRevenueDto Revenue { get; set; } = new();
        public SellerOrderStatsDto Orders { get; set; } = new();
        public SellerProductStatsDto Products { get; set; } = new();
        public List<TopProductDto> TopProducts { get; set; } = [];
        public List<RevenueTrendPointDto> RevenueTrend { get; set; } = [];
    }

    public class SellerRevenueDto
    {
        /// <summary>Total revenue in the requested trend window (default 30 days).</summary>
        public decimal Total { get; set; }

        /// <summary>Revenue in the previous equal-length window for growth calculation.</summary>
        public decimal PreviousPeriod { get; set; }

        /// <summary>Percentage change vs previous period. Null when previous period is 0.</summary>
        public double? GrowthPercent { get; set; }

        public decimal AverageOrderValue { get; set; }
    }

    public class SellerOrderStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Shipped { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
    }

    public class SellerProductStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int OutOfStock { get; set; }
        public int LowStock { get; set; }
    }

    public class TopProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal Revenue { get; set; }
        public string? ThumbnailUrl { get; set; }
    }

    public class RevenueTrendPointDto
    {
        public DateOnly Date { get; set; }
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }
}
