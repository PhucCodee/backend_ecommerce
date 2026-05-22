using ECommerce.Application.DTOs.product;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface IProductSkuService
    {
        Task<ProductSkuDto> CreateAsync(ProductSkuCreateDto createDto, int? sellerId = null);
        Task<ProductSkuDto> UpdateAsync(int skuId, ProductSkuUpdateDto updateDto, int? sellerId = null);
        Task<bool> DeleteAsync(int skuId, int? sellerId = null);
        Task<ProductSkuDto> RestoreAsync(int skuId, int? sellerId = null);
    }
}