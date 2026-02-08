using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository(ApplicationDbContext context) : Repository<Product>(context), IProductRepository
    {
        private IQueryable<Product> GetActiveProducts() => _context.Products.Where(p => p.RemovedAt == null);

        private IQueryable<Product> GetActiveProductsWithDetails() =>
            GetActiveProducts()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory!)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductMetrics);

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await GetActiveProducts()
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            return await GetActiveProducts()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Where(p => p.ProductName.Contains(searchTerm) ||
                           (p.Description != null && p.Description.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryNameAsync(string categoryName)
        {
            return await GetActiveProducts()
                .Include(p => p.Category)
                .Where(p => p.Category.CategoryName == categoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetAllWithDetailsAsync()
        {
            return await GetActiveProductsWithDetails().ToListAsync();
        }

        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await GetActiveProductsWithDetails()
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public override async Task<Product?> GetByIdAsync(int productId)
        {
            return await GetActiveProducts()
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await GetActiveProducts().ToListAsync();
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = GetActiveProductsWithDetails();

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetBySellerPagedAsync(int sellerId, int pageNumber, int pageSize)
        {
            var query = GetActiveProductsWithDetails()
                .Where(p => p.SellerId == sellerId);

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null)
        {
            var query = _context.Products.Where(p => p.Slug == slug);
            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.ProductId != excludeProductId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredPagedAsync(
            int pageNumber,
            int pageSize,
            string? sortBy = null,
            bool desc = false,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? categoryId = null,
            string? brand = null,
            int? sellerId = null,
            string? status = null,
            string? search = null,
            bool primaryOnly = true,
            bool? inStock = null)
        {
            var query = GetActiveProductsWithDetails();

            // Apply filters
            query = ApplyFilters(query, minPrice, maxPrice, categoryId, brand, sellerId, status, search, primaryOnly, inStock);

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, sortBy, desc);

            // Apply pagination
            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }

        private static IQueryable<Product> ApplyFilters(
            IQueryable<Product> query,
            decimal? minPrice,
            decimal? maxPrice,
            int? categoryId,
            string? brand,
            int? sellerId,
            string? status,
            string? search,
            bool primaryOnly,
            bool? inStock)
        {
            // Filter by price range (using default SKU price)
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.ProductSkus
                    .Any(s => s.IsDefault && s.Price >= minPrice.Value));
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.ProductSkus
                    .Any(s => s.IsDefault && s.Price <= maxPrice.Value));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Filter by brand
            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(p => p.Brand != null && p.Brand.ToLower().Contains(brand.ToLower()));
            }

            // Filter by seller
            if (sellerId.HasValue)
            {
                query = query.Where(p => p.SellerId == sellerId.Value);
            }

            // Filter by status
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ProductStatus>(status, true, out var productStatus))
            {
                query = query.Where(p => p.Status == productStatus);
            }

            // Search by name or description
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(searchLower) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(searchLower)));
            }

            // Filter by primary only (has default SKU)
            if (primaryOnly)
            {
                query = query.Where(p => p.ProductSkus.Any(s => s.IsDefault));
            }

            // Filter by in-stock
            if (inStock.HasValue && inStock.Value)
            {
                query = query.Where(p => p.ProductSkus
                    .Any(s => s.IsDefault && s.Inventory != null && s.Inventory.QuantityAvailable > 0));
            }

            return query;
        }

        private static IQueryable<Product> ApplySorting(IQueryable<Product> query, string? sortBy, bool desc)
        {
            return sortBy?.ToLower() switch
            {
                "price" => desc
                    ? query.OrderByDescending(p => p.ProductSkus.Where(s => s.IsDefault).Select(s => s.Price).FirstOrDefault())
                    : query.OrderBy(p => p.ProductSkus.Where(s => s.IsDefault).Select(s => s.Price).FirstOrDefault()),

                "name" or "a-z" => desc
                    ? query.OrderByDescending(p => p.ProductName)
                    : query.OrderBy(p => p.ProductName),

                "rating" => desc
                    ? query.OrderByDescending(p => p.ProductMetrics.Select(m => m.AverageRating).FirstOrDefault())
                    : query.OrderBy(p => p.ProductMetrics.Select(m => m.AverageRating).FirstOrDefault()),

                "popularity" or "views" => desc
                    ? query.OrderByDescending(p => p.ProductMetrics.Select(m => m.ViewCount).FirstOrDefault())
                    : query.OrderBy(p => p.ProductMetrics.Select(m => m.ViewCount).FirstOrDefault()),

                "sales" => desc
                    ? query.OrderByDescending(p => p.ProductMetrics.Select(m => m.PurchaseCount).FirstOrDefault())
                    : query.OrderBy(p => p.ProductMetrics.Select(m => m.PurchaseCount).FirstOrDefault()),

                "newest" or "createdat" => query.OrderByDescending(p => p.CreatedAt),

                "oldest" => query.OrderBy(p => p.CreatedAt),

                _ => query.OrderByDescending(p => p.CreatedAt) // Default: newest first
            };
        }
    }
}