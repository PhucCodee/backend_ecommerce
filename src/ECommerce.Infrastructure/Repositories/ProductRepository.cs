using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductRepository(ApplicationDbContext context) : Repository<Product>(context), IProductRepository
    {
        public async Task<Product?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Products
                .Where(p => p.RemovedAt == null)
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
            return await _context.Products
                .Where(p => p.RemovedAt == null)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
        }

        public override async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Where(p => p.RemovedAt == null)
                .ToListAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null)
        {
            var query = _context.Products.Where(p => p.Slug == slug);
            if (excludeProductId.HasValue)
                query = query.Where(p => p.ProductId != excludeProductId.Value);
            return await query.AnyAsync();
        }
    }
}