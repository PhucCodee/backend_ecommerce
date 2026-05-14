using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository(ApplicationDbContext context)
        : Repository<Product>(context),
            IProductRepository
    {
        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await _context
                .Products.Where(p => p.RemovedAt == null)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.ProductImages.Where(img => !img.IsDeleted))
                .Include(p => p.ProductMetrics)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public override async Task<Product?> GetByIdAsync(int productId)
        {
            return await _context
                .Products.Where(p => p.RemovedAt == null)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Where(p => p.RemovedAt == null).ToListAsync();
        }

        public async Task<Product?> GetByIdIncludingRemovedAsync(int id)
        {
            return await _context
                .Products.Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.ProductImages.Where(img => !img.IsDeleted))
                .Include(p => p.ProductMetrics)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null)
        {
            // The DB-level unique constraint on products.slug is not filtered by
            // RemovedAt, so we must check every row (including soft-deleted ones)
            // otherwise the caller can generate a slug that still collides on INSERT.
            var query = _context.Products.IgnoreQueryFilters().Where(p => p.Slug == slug);
            if (excludeProductId.HasValue)
                query = query.Where(p => p.ProductId != excludeProductId.Value);
            return await query.AnyAsync();
        }
    }
}
