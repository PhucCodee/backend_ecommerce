using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.coupon;

namespace ECommerce.Application.Interfaces
{
    public interface ICouponService
    {
        Task<CouponDto> CreateAsync(CouponCreateDto createDto);
        Task<CouponDto> UpdateAsync(int couponId, CouponUpdateDto updateDto);
        Task<bool> DeleteAsync(int couponId);
        Task<PagedResult<CouponDto>> GetPagedAsync(PaginationParams paginationParams);
        Task<CouponDto> GetByCodeAsync(string code, decimal? subtotal = null);
    }
}
