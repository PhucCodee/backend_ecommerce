using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.productsku;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IProductSkuService
    {
        Task<ProductSkuDetailDto> GetByIdAsync(int skuId);
        Task<IEnumerable<ProductSkuDetailDto>> GetByProductIdAsync(int productId);
        Task<PagedResult<ProductSkuDetailDto>> GetByProductIdPagedAsync(int productId, PaginationParams paginationParams);
        Task<ProductSkuDetailDto> CreateAsync(ProductSkuCreateDto createDto);
        Task<ProductSkuDetailDto> UpdateAsync(int skuId, ProductSkuUpdateDto updateDto);
        Task<bool> DeleteAsync(int skuId);
        Task<PagedResult<ProductSkuDetailDto>> GetBySellerPagedAsync(int sellerId, PaginationParams paginationParams);
        Task<ProductSkuDetailDto> CreateSellerSkuAsync(ProductSkuCreateDto createDto, int sellerId);
        Task<ProductSkuDetailDto> UpdateSellerSkuAsync(int skuId, int sellerId, ProductSkuUpdateDto updateDto);
        Task<bool> DeleteSellerSkuAsync(int skuId, int sellerId);
    }
}