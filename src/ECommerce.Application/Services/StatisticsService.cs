using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.statistics;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services
{
    public class StatisticsService(ApplicationDbContext db) : IStatisticsService
    {
        // ─────────────────────────────────────────────────────────────
        //  SELLER
        // ─────────────────────────────────────────────────────────────

        public async Task<SellerDashboardDto> GetSellerDashboardAsync(int sellerId, int trendDays = 30)
        {
            var now = DateTime.UtcNow;
            var periodStart = now.AddDays(-trendDays);
            var prevStart = periodStart.AddDays(-trendDays);

            // ── Revenue ──────────────────────────────────────────────
            var currentRevenue = await db.OrderItems
                .Where(oi => oi.SellerId == sellerId
                    && oi.Order.Status != OrderStatus.cancelled
                    && oi.Order.Status != OrderStatus.failed
                    && oi.Order.CreatedAt >= periodStart)
                .SumAsync(oi => (decimal?)oi.Subtotal) ?? 0m;

            var prevRevenue = await db.OrderItems
                .Where(oi => oi.SellerId == sellerId
                    && oi.Order.Status != OrderStatus.cancelled
                    && oi.Order.Status != OrderStatus.failed
                    && oi.Order.CreatedAt >= prevStart
                    && oi.Order.CreatedAt < periodStart)
                .SumAsync(oi => (decimal?)oi.Subtotal) ?? 0m;

            double? growthPercent = prevRevenue == 0
                ? null
                : Math.Round((double)((currentRevenue - prevRevenue) / prevRevenue * 100), 2);

            // avg order value based on distinct orders in period
            var ordersInPeriod = await db.Orders
                .Where(o => o.OrderItems.Any(oi => oi.SellerId == sellerId)
                    && o.Status != OrderStatus.cancelled
                    && o.Status != OrderStatus.failed
                    && o.CreatedAt >= periodStart)
                .CountAsync();

            var avgOrderValue = ordersInPeriod > 0
                ? Math.Round(currentRevenue / ordersInPeriod, 2)
                : 0m;

            // ── Orders ───────────────────────────────────────────────
            var sellerOrderIds = await db.OrderItems
                .Where(oi => oi.SellerId == sellerId)
                .Select(oi => oi.OrderId)
                .Distinct()
                .ToListAsync();

            var orderStatuses = await db.Orders
                .Where(o => sellerOrderIds.Contains(o.OrderId))
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var orders = new SellerOrderStatsDto
            {
                Total = orderStatuses.Sum(g => g.Count),
                Pending = orderStatuses.Where(g => g.Status == OrderStatus.created || g.Status == OrderStatus.confirmed).Sum(g => g.Count),
                Processing = orderStatuses.Where(g => g.Status == OrderStatus.processing).Sum(g => g.Count),
                Shipped = orderStatuses.Where(g => g.Status == OrderStatus.shipped).Sum(g => g.Count),
                Delivered = orderStatuses.Where(g => g.Status == OrderStatus.delivered).Sum(g => g.Count),
                Cancelled = orderStatuses.Where(g => g.Status == OrderStatus.cancelled || g.Status == OrderStatus.failed).Sum(g => g.Count),
            };

            // ── Products ─────────────────────────────────────────────
            var productStats = await db.Products
                .Where(p => p.SellerId == sellerId && p.RemovedAt == null)
                .Select(p => new
                {
                    IsActive = p.Status == ProductStatus.active,
                    IsOutOfStock = p.ProductSkus.Any(s => s.IsActive && s.IsDefault && (s.Inventory == null || s.Inventory.QuantityAvailable == 0)),
                    IsLowStock = p.ProductSkus.Any(s => s.IsActive && s.IsDefault && s.Inventory != null
                        && s.Inventory.QuantityAvailable > 0
                        && s.Inventory.QuantityAvailable <= s.Inventory.ReorderPoint),
                })
                .ToListAsync();

            var products = new SellerProductStatsDto
            {
                Total = productStats.Count,
                Active = productStats.Count(p => p.IsActive),
                OutOfStock = productStats.Count(p => p.IsOutOfStock),
                LowStock = productStats.Count(p => p.IsLowStock),
            };

            // ── Top Products ─────────────────────────────────────────
            var topProducts = await db.OrderItems
                .Where(oi => oi.SellerId == sellerId
                    && oi.Order.Status != OrderStatus.cancelled
                    && oi.Order.Status != OrderStatus.failed
                    && oi.Order.CreatedAt >= periodStart)
                .GroupBy(oi => new { oi.SkuId, oi.ProductName, oi.Sku })
                .Select(g => new
                {
                    g.Key.SkuId,
                    g.Key.ProductName,
                    g.Key.Sku,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Subtotal),
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            // fetch thumbnails for top products
            var skuIds = topProducts.Select(t => t.SkuId).ToList();
            var thumbnails = await db.ProductImages
                .Where(pi => skuIds.Contains(pi.SkuId) && !pi.IsDeleted && pi.IsPrimary)
                .Select(pi => new { pi.SkuId, pi.ThumbnailUrl })
                .ToListAsync();

            var thumbMap = thumbnails.ToDictionary(t => t.SkuId, t => t.ThumbnailUrl);

            // get productId via ProductSku
            var skuProductMap = await db.ProductSkus
                .Where(ps => skuIds.Contains(ps.SkuId))
                .Select(ps => new { ps.SkuId, ps.ProductId })
                .ToDictionaryAsync(ps => ps.SkuId, ps => ps.ProductId);

            var topProductDtos = topProducts.Select(t => new TopProductDto
            {
                ProductId = skuProductMap.TryGetValue(t.SkuId, out var pid) ? pid : 0,
                ProductName = t.ProductName,
                Sku = t.Sku,
                UnitsSold = t.UnitsSold,
                Revenue = t.Revenue,
                ThumbnailUrl = thumbMap.TryGetValue(t.SkuId, out var url) ? url : null,
            }).ToList();

            // ── Revenue Trend ────────────────────────────────────────
            var rawTrend = await db.OrderItems
                .Where(oi => oi.SellerId == sellerId
                    && oi.Order.Status != OrderStatus.cancelled
                    && oi.Order.Status != OrderStatus.failed
                    && oi.Order.CreatedAt >= periodStart)
                .GroupBy(oi => oi.Order.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.Subtotal),
                    Orders = g.Select(x => x.OrderId).Distinct().Count(),
                })
                .ToListAsync();

            var trendMap = rawTrend.ToDictionary(t => t.Date);
            var trend = Enumerable.Range(0, trendDays)
                .Select(i => periodStart.AddDays(i).Date)
                .Select(date => new RevenueTrendPointDto
                {
                    Date = DateOnly.FromDateTime(date),
                    Revenue = trendMap.TryGetValue(date, out var pt) ? pt.Revenue : 0m,
                    Orders = trendMap.TryGetValue(date, out var pt2) ? pt2.Orders : 0,
                })
                .ToList();

            return new SellerDashboardDto
            {
                Revenue = new SellerRevenueDto
                {
                    Total = currentRevenue,
                    PreviousPeriod = prevRevenue,
                    GrowthPercent = growthPercent,
                    AverageOrderValue = avgOrderValue,
                },
                Orders = orders,
                Products = products,
                TopProducts = topProductDtos,
                RevenueTrend = trend,
            };
        }

        // ─────────────────────────────────────────────────────────────
        //  ADMIN
        // ─────────────────────────────────────────────────────────────

        public async Task<AdminDashboardDto> GetAdminDashboardAsync(int trendDays = 30)
        {
            var now = DateTime.UtcNow;
            var periodStart = now.AddDays(-trendDays);
            var prevStart = periodStart.AddDays(-trendDays);

            // ── Revenue ──────────────────────────────────────────────
            var currentRevenue = await db.Orders
                .Where(o => o.Status != OrderStatus.cancelled
                    && o.Status != OrderStatus.failed
                    && o.CreatedAt >= periodStart)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            var prevRevenue = await db.Orders
                .Where(o => o.Status != OrderStatus.cancelled
                    && o.Status != OrderStatus.failed
                    && o.CreatedAt >= prevStart
                    && o.CreatedAt < periodStart)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            double? growthPercent = prevRevenue == 0
                ? null
                : Math.Round((double)((currentRevenue - prevRevenue) / prevRevenue * 100), 2);

            var ordersInPeriod = await db.Orders
                .CountAsync(o => o.Status != OrderStatus.cancelled
                    && o.Status != OrderStatus.failed
                    && o.CreatedAt >= periodStart);

            var avgOrderValue = ordersInPeriod > 0
                ? Math.Round(currentRevenue / ordersInPeriod, 2)
                : 0m;

            // ── Orders ───────────────────────────────────────────────
            var orderStatuses = await db.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var adminOrders = new AdminOrderStatsDto
            {
                Total = orderStatuses.Sum(g => g.Count),
                Pending = orderStatuses.Where(g => g.Status == OrderStatus.created || g.Status == OrderStatus.confirmed).Sum(g => g.Count),
                Processing = orderStatuses.Where(g => g.Status == OrderStatus.processing).Sum(g => g.Count),
                Shipped = orderStatuses.Where(g => g.Status == OrderStatus.shipped).Sum(g => g.Count),
                Delivered = orderStatuses.Where(g => g.Status == OrderStatus.delivered).Sum(g => g.Count),
                Cancelled = orderStatuses.Where(g => g.Status == OrderStatus.cancelled || g.Status == OrderStatus.failed).Sum(g => g.Count),
            };

            // ── Users ────────────────────────────────────────────────
            var totalUsers = await db.Users.CountAsync(u => u.DeletedAt == null);

            var roleStats = await db.UserRoles
                .Where(r => r.RevokedAt == null)
                .GroupBy(r => r.Role)
                .Select(g => new { Role = g.Key, Count = g.Select(r => r.UserId).Distinct().Count() })
                .ToListAsync();

            var newUsers = await db.Users
                .CountAsync(u => u.DeletedAt == null && u.CreatedAt >= periodStart);

            var adminUsers = new AdminUserStatsDto
            {
                TotalUsers = totalUsers,
                TotalSellers = roleStats.FirstOrDefault(r => r.Role == UserRoleType.seller)?.Count ?? 0,
                TotalBuyers = roleStats.FirstOrDefault(r => r.Role == UserRoleType.buyer)?.Count ?? 0,
                NewUsersThisPeriod = newUsers,
            };

            // ── Products ─────────────────────────────────────────────
            var totalProducts = await db.Products.CountAsync(p => p.RemovedAt == null);
            var activeProducts = await db.Products.CountAsync(p => p.RemovedAt == null && p.Status == ProductStatus.active);
            var outOfStock = await db.Products
                .CountAsync(p => p.RemovedAt == null
                    && p.ProductSkus.Any(s => s.IsActive && s.IsDefault
                        && (s.Inventory == null || s.Inventory.QuantityAvailable == 0)));

            var adminProducts = new AdminProductStatsDto
            {
                Total = totalProducts,
                Active = activeProducts,
                OutOfStock = outOfStock,
            };

            // ── Top Sellers ──────────────────────────────────────────
            var topSellers = await db.OrderItems
                .Where(oi => oi.Order.Status != OrderStatus.cancelled
                    && oi.Order.Status != OrderStatus.failed
                    && oi.Order.CreatedAt >= periodStart)
                .GroupBy(oi => new { oi.SellerId, oi.Seller.Username })
                .Select(g => new TopSellerDto
                {
                    SellerId = g.Key.SellerId,
                    SellerName = g.Key.Username,
                    TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                    TotalRevenue = g.Sum(oi => oi.Subtotal),
                    TotalProductsSold = g.Sum(oi => oi.Quantity),
                })
                .OrderByDescending(s => s.TotalRevenue)
                .Take(5)
                .ToListAsync();

            // ── Top Products (platform-wide) ─────────────────────────
            var topProductsRaw = await db.OrderItems
                .Where(oi => oi.Order.Status != OrderStatus.cancelled
                    && oi.Order.Status != OrderStatus.failed
                    && oi.Order.CreatedAt >= periodStart)
                .GroupBy(oi => new { oi.SkuId, oi.ProductName, oi.Sku })
                .Select(g => new
                {
                    g.Key.SkuId,
                    g.Key.ProductName,
                    g.Key.Sku,
                    UnitsSold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.Subtotal),
                })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToListAsync();

            var skuIds = topProductsRaw.Select(t => t.SkuId).ToList();
            var thumbnails = await db.ProductImages
                .Where(pi => skuIds.Contains(pi.SkuId) && !pi.IsDeleted && pi.IsPrimary)
                .Select(pi => new { pi.SkuId, pi.ThumbnailUrl })
                .ToListAsync();
            var thumbMap = thumbnails.ToDictionary(t => t.SkuId, t => t.ThumbnailUrl);

            var skuProductMap = await db.ProductSkus
                .Where(ps => skuIds.Contains(ps.SkuId))
                .Select(ps => new { ps.SkuId, ps.ProductId })
                .ToDictionaryAsync(ps => ps.SkuId, ps => ps.ProductId);

            var topProductDtos = topProductsRaw.Select(t => new TopProductDto
            {
                ProductId = skuProductMap.TryGetValue(t.SkuId, out var pid) ? pid : 0,
                ProductName = t.ProductName,
                Sku = t.Sku,
                UnitsSold = t.UnitsSold,
                Revenue = t.Revenue,
                ThumbnailUrl = thumbMap.TryGetValue(t.SkuId, out var url) ? url : null,
            }).ToList();

            // ── Revenue Trend ────────────────────────────────────────
            var rawTrend = await db.Orders
                .Where(o => o.Status != OrderStatus.cancelled
                    && o.Status != OrderStatus.failed
                    && o.CreatedAt >= periodStart)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.TotalAmount),
                    Orders = g.Count(),
                })
                .ToListAsync();

            var trendMap = rawTrend.ToDictionary(t => t.Date);
            var trend = Enumerable.Range(0, trendDays)
                .Select(i => periodStart.AddDays(i).Date)
                .Select(date => new RevenueTrendPointDto
                {
                    Date = DateOnly.FromDateTime(date),
                    Revenue = trendMap.TryGetValue(date, out var pt) ? pt.Revenue : 0m,
                    Orders = trendMap.TryGetValue(date, out var pt2) ? pt2.Orders : 0,
                })
                .ToList();

            return new AdminDashboardDto
            {
                Revenue = new AdminRevenueDto
                {
                    Total = currentRevenue,
                    PreviousPeriod = prevRevenue,
                    GrowthPercent = growthPercent,
                    AverageOrderValue = avgOrderValue,
                },
                Orders = adminOrders,
                Users = adminUsers,
                Products = adminProducts,
                TopSellers = topSellers,
                TopProducts = topProductDtos,
                RevenueTrend = trend,
            };
        }
    }
}
