using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface ICouponRepository : IRepository<Coupon>
    {
        Task<Coupon?> GetByCodeAsync(string code);
        Task<int> CountUsageAsync(int couponId);
        Task<(IEnumerable<Coupon> Coupons, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize
        );
    }
}
