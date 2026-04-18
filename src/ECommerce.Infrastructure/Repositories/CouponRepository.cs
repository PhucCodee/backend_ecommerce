using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories
{
    public class CouponRepository(ApplicationDbContext context) : Repository<Coupon>(context), ICouponRepository
    {
        public async Task<Coupon?> GetByCodeAsync(string code)
        {
            var normalized = code.Trim().ToUpperInvariant();
            return await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == normalized);
        }

        public async Task<int> CountUsageAsync(int couponId)
        {
            return await _context.Orders.CountAsync(o => o.CouponId == couponId);
        }
    }
}