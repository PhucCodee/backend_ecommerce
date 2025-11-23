using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await Task.FromResult(new List<ProductDto>
            {
                new ProductDto
                {
                    Id = 0,
                    Name = "Test Product",
                    Description = "A placeholder product",
                    Price = 9.99m,
                    Stock = 100,
                    ImageUrl = "https://example.com/image.png"
                }
            });
            return Ok(products);
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            if (id == 0)
                return NotFound();

            var product = await Task.FromResult(new ProductDto
            {
                Id = 0,
                Name = "Test Product",
                Description = "A placeholder product",
                Price = 9.99m,
                Stock = 100,
                ImageUrl = "https://example.com/image.png"
            });
            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductDto productDto)
        {
            productDto.Id = 0;
            var createdProduct = await Task.FromResult(productDto);
            return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto productDto)
        {
            if (id != productDto.Id)
                return BadRequest();

            await Task.CompletedTask; // Placeholder
            return NoContent();
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await Task.CompletedTask; // Placeholder
            return NoContent();
        }
    }
}