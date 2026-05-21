using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class ProductSkuRepository(ApplicationDbContext context)
        : Repository<ProductSku>(context),
            IProductSkuRepository
    {
        private IQueryable<ProductSku> GetActiveSkus() =>
            _context.ProductSkus.Where(ps => ps.IsActive && ps.Product.RemovedAt == null);

        public async Task<ProductSku?> GetByIdWithDetailsAsync(int skuId)
        {
            return await _context
                .ProductSkus.Include(ps => ps.Product)
                .Include(ps => ps.Inventory)
                .Include(ps => ps.ProductImages)
                .FirstOrDefaultAsync(ps => ps.SkuId == skuId);
        }

        public async Task<IEnumerable<ProductSku>> GetByProductIdAsync(int productId)
        {
            return await GetActiveSkus().Where(ps => ps.ProductId == productId).ToListAsync();
        }

        public async Task<bool> HasActiveOrdersAsync(int skuId)
        {
            return await _context.OrderItems.AnyAsync(oi =>
                oi.SkuId == skuId
                && (
                    oi.Order.Status == OrderStatus.created
                    || oi.Order.Status == OrderStatus.confirmed
                    || oi.Order.Status == OrderStatus.processing
                    || oi.Order.Status == OrderStatus.shipped
                )
            );
        }

        public async Task<IEnumerable<ProductSku>> GetByProductIdWithDetailsAsync(int productId)
        {
            return await GetActiveSkus()
                .Include(ps => ps.Product)
                .Include(ps => ps.Inventory)
                .Include(ps => ps.ProductImages)
                .Where(ps => ps.ProductId == productId)
                .OrderByDescending(ps => ps.IsDefault)
                .ThenBy(ps => ps.CreatedAt)
                .ToListAsync();
        }

        public async Task<ProductSku?> GetBySkuCodeAsync(string skuCode)
        {
            return await GetActiveSkus()
                .Include(ps => ps.Product)
                .Include(ps => ps.Inventory)
                .FirstOrDefaultAsync(ps => ps.Sku == skuCode);
        }

        public async Task<(IEnumerable<ProductSku> Skus, int TotalCount)> GetByProductIdPagedAsync(
            int productId,
            int pageNumber,
            int pageSize
        )
        {
            var query = GetActiveSkus()
                .Include(ps => ps.Product)
                .Include(ps => ps.Inventory)
                .Include(ps => ps.ProductImages)
                .Where(ps => ps.ProductId == productId);

            var totalCount = await query.CountAsync();

            var skus = await query
                .OrderByDescending(ps => ps.IsDefault)
                .ThenByDescending(ps => ps.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (skus, totalCount);
        }

        public async Task<(IEnumerable<ProductSku> Skus, int TotalCount)> GetBySellerPagedAsync(
            int sellerId,
            int pageNumber,
            int pageSize
        )
        {
            var query = GetActiveSkus()
                .Include(ps => ps.Product)
                .Include(ps => ps.Inventory)
                .Include(ps => ps.ProductImages)
                .Where(ps => ps.Product.SellerId == sellerId);

            var totalCount = await query.CountAsync();

            var skus = await query
                .OrderByDescending(ps => ps.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (skus, totalCount);
        }

        public override async Task<ProductSku?> GetByIdAsync(int skuId)
        {
            return await _context
                .ProductSkus.Include(ps => ps.Product)
                .FirstOrDefaultAsync(ps => ps.SkuId == skuId);
        }
    }
}
