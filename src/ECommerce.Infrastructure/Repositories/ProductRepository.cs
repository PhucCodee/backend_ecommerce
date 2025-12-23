using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository(ApplicationDbContext context) : Repository<Product>(context), IProductRepository
    {
        private IQueryable<Product> GetActiveProducts() => _context.Products.Where(p => p.RemovedAt == null);

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
            return await GetActiveProducts()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory!)
                .Include(p => p.ProductImages)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await GetActiveProducts()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory!)
                .Include(p => p.ProductImages)
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
            var query = GetActiveProducts()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory!)
                .Include(p => p.ProductImages);

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
            var query = GetActiveProducts()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory!)
                .Include(p => p.ProductImages)
                .Where(p => p.SellerId == sellerId);

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (products, totalCount);
        }
    }
}