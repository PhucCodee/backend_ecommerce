using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.product;

namespace ECommerce.Application.Interfaces
{
    public interface IProductQueryService
    {
        Task<ProductDetailDto> GetByIdAsync(int productId);
        Task<PagedResult<ProductSummaryDto>> GetFilteredAsync(ProductQueryParams productQueryParams);
    }
}