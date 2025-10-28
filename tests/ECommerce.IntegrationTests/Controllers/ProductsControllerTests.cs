using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using ECommerce.API.Controllers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.DTOs;
using System.Collections.Generic;

namespace ECommerce.IntegrationTests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly ProductsController _controller;
        private readonly Mock<IProductService> _mockProductService;

        public ProductsControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _controller = new ProductsController(_mockProductService.Object);
        }

        [Fact]
        public async Task GetAllProducts_ReturnsOkResult()
        {
            // Arrange
            var products = new List<ProductDto>
            {
                new ProductDto { Id = 1, Name = "Product 1", Price = 10.0m },
                new ProductDto { Id = 2, Name = "Product 2", Price = 20.0m }
            };
            _mockProductService.Setup(service => service.GetAllProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await _controller.GetAllProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, okResult.StatusCode);
            var returnProducts = Assert.IsType<List<ProductDto>>(okResult.Value);
            Assert.Equal(2, returnProducts.Count);
        }

        [Fact]
        public async Task GetProductById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            int productId = 99; // Non-existing product ID
            _mockProductService.Setup(service => service.GetProductByIdAsync(productId)).ReturnsAsync((ProductDto)null);

            // Act
            var result = await _controller.GetProductById(productId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateProduct_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var newProduct = new ProductDto { Name = "New Product", Price = 15.0m };
            _mockProductService.Setup(service => service.CreateProductAsync(newProduct)).ReturnsAsync(newProduct);

            // Act
            var result = await _controller.CreateProduct(newProduct);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, createdResult.StatusCode);
            Assert.Equal(newProduct, createdResult.Value);
        }
    }
}