using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> GetProductByIdAsync(int productId);
        Task<ProductDto> CreateProductAsync(ProductDto productDto);
        Task<ProductDto> UpdateProductAsync(int productId, ProductDto productDto);
        Task<bool> DeleteProductAsync(int productId);
    }
}