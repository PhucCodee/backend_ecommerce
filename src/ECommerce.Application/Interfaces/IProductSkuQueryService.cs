using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.product;

namespace ECommerce.Application.Interfaces
{
    public interface IProductSkuQueryService
    {
        Task<ProductSkuDto> GetByIdAsync(int skuId);
        Task<PagedResult<ProductSkuDto>> GetByProductIdPagedAsync(int productId, PaginationParams paginationParams);
        Task<PagedResult<ProductSkuDto>> GetBySellerPagedAsync(int sellerId, PaginationParams paginationParams);
    }
}