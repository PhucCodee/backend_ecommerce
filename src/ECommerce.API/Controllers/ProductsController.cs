using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Common.Responses;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        private readonly IProductService _productService = productService;

        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(products));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound(ApiResponse<ProductDto>.Failure("Product not found", StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<ProductDto>.SuccessResponse(product));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
        {
            try
            {
                var createdProduct = await _productService.CreateProductAsync(productDto);
                var response = ApiResponse<ProductDto>.SuccessResponse(createdProduct, "Product created", StatusCodes.Status201Created);
                return StatusCode(StatusCodes.Status201Created, response);
            }
            catch (InvalidOperationException ex)
            {
                var response = ApiResponse<ProductDto>.Failure(ex.Message, StatusCodes.Status400BadRequest);
                return BadRequest(response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
        {
            try
            {
                var updated = await _productService.UpdateProductAsync(id, productDto);
                if (updated == null)
                {
                    return NotFound(ApiResponse<ProductDto>.Failure("Product not found", StatusCodes.Status404NotFound));
                }

                var response = ApiResponse<ProductDto>.SuccessResponse(updated, "Product updated");
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                var response = ApiResponse<ProductDto>.Failure(ex.Message, StatusCodes.Status400BadRequest);
                return BadRequest(response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteProductAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<object>.Failure("Product not found", StatusCodes.Status404NotFound));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null, "Product deleted"));
        }
    }
}