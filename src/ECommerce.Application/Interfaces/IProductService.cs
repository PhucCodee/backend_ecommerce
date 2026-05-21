using System.Threading.Tasks;
using ECommerce.Application.DTOs.product;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductDto> CreateAsync(ProductCreateDto createDto, int sellerId);
        Task<ProductDto> UpdateAsync(int productId, ProductUpdateDto updateDto, int? sellerId = null);
        Task<bool> DeleteAsync(int productId, int? sellerId = null);
        Task<ProductDto> RestoreAsync(int productId, int? sellerId = null);
    }
}