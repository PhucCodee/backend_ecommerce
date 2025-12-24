using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.product;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDetailDto> GetByIdAsync(int productId);
        Task<IEnumerable<ProductDetailDto>> GetAllAsync();
        Task<PagedResult<ProductDetailDto>> GetAllPagedAsync(PaginationParams paginationParams);
        Task<ProductDetailDto> CreateAsync(ProductCreateDto createDto, int sellerId);
        Task<ProductDetailDto> UpdateAsync(int productId, ProductUpdateDto updateDto);
        Task<bool> DeleteAsync(int productId);
        Task<PagedResult<ProductDetailDto>> GetBySellerPagedAsync(int sellerId, PaginationParams paginationParams);
        Task<ProductDetailDto> UpdateSellerProductAsync(int productId, int sellerId, ProductUpdateDto updateDto);
        Task<bool> DeleteSellerProductAsync(int productId, int sellerId);
    }
}