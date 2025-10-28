using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.DTOs;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        // Method to get all products
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();

        // Method to get a product by its ID
        Task<ProductDto> GetProductByIdAsync(Guid productId);

        // Method to create a new product
        Task<ProductDto> CreateProductAsync(ProductDto productDto);

        // Method to update an existing product
        Task<ProductDto> UpdateProductAsync(Guid productId, ProductDto productDto);

        // Method to delete a product
        Task<bool> DeleteProductAsync(Guid productId);
    }
}