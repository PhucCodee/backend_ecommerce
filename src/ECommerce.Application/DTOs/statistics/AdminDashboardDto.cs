using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.statistics
{
    public class AdminDashboardDto
    {
        public AdminRevenueDto Revenue { get; set; } = new();
        public AdminOrderStatsDto Orders { get; set; } = new();
        public AdminUserStatsDto Users { get; set; } = new();
        public AdminProductStatsDto Products { get; set; } = new();
        public List<TopSellerDto> TopSellers { get; set; } = [];
        public List<TopProductDto> TopProducts { get; set; } = [];
        public List<RevenueTrendPointDto> RevenueTrend { get; set; } = [];
    }

    public class AdminRevenueDto
    {
        public decimal Total { get; set; }
        public decimal PreviousPeriod { get; set; }
        public double? GrowthPercent { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class AdminOrderStatsDto
    {
        public int Total { get; set; }
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Shipped { get; set; }
        public int Delivered { get; set; }
        public int Cancelled { get; set; }
    }

    public class AdminUserStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalSellers { get; set; }
        public int TotalBuyers { get; set; }
        public int NewUsersThisPeriod { get; set; }
    }

    public class AdminProductStatsDto
    {
        public int Total { get; set; }
        public int Active { get; set; }
        public int OutOfStock { get; set; }
    }

    public class TopSellerDto
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalProductsSold { get; set; }
    }
}
