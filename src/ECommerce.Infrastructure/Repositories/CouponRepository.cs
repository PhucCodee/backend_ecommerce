using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class CouponRepository(ApplicationDbContext context)
        : Repository<Coupon>(context),
            ICouponRepository
    {
        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            var normalized = code.Trim().ToUpperInvariant();
            return await _context.Coupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == normalized);
        }

        public async Task<int> CountUsageAsync(int couponId)
        {
            return await _context.Orders.CountAsync(o => o.CouponId == couponId);
        }

        public async Task<(IEnumerable<Coupon> Coupons, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize
        )
        {
            var query = _context.Coupons.OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync();

            var coupons = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (coupons, totalCount);
        }
    }
}
