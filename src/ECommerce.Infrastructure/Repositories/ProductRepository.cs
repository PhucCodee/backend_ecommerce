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
        private IQueryable<Product> ProductDetailsQuery(bool includeRemoved = false)
        {
            var query = _context.Products.AsQueryable();

            if (!includeRemoved)
                query = query.Where(p => p.RemovedAt == null);

            return query
                .AsSplitQuery()
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.ProductImages.Where(img => !img.IsDeleted))
                .Include(p => p.ProductMetrics);
        }

        public async Task<Product?> GetByIdIncludingRemovedAsync(int id)
        {
            return await ProductDetailsQuery(includeRemoved: true)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await ProductDetailsQuery(includeRemoved: false)
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

        public async Task<bool> HasOrderItemsAsync(int productId)
        {
            // Any historical order line tied to one of this product's SKUs
            // forces us to keep the product around (order_items.sku_id is
            // ON DELETE RESTRICT and carries the buyer-facing snapshot).
            return await _context.ProductSkus
                .Where(s => s.ProductId == productId)
                .AnyAsync(s => s.OrderItems.Any());
        }
    }
}
