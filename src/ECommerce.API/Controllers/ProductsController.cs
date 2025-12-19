using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Common.Responses;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.Ok(products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            return Ok(ApiResponse<ProductDto>.Ok(product));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductDto dto)
        {
            var created = await _productService.CreateProductAsync(dto);
            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<ProductDto>.Ok(created, "Product created")
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto dto)
        {
            var updated = await _productService.UpdateProductAsync(id, dto);
            return Ok(ApiResponse<ProductDto>.Ok(updated, "Product updated"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);
            return Ok(ApiResponse<object>.Ok(new { id }, "Product deleted successfully"));
        }
    }
}