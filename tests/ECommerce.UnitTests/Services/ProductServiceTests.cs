using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;

namespace ECommerce.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly IProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _productService = new ProductService(_mockProductRepository.Object);
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.0m },
                new Product { Id = 2, Name = "Product 2", Price = 20.0m }
            };
            _mockProductRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetProductById_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1", Price = 10.0m };
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
        }

        [Fact]
        public async Task GetProductById_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Product)null);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProduct_ShouldAddProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1", Price = 10.0m };
            _mockProductRepository.Setup(repo => repo.AddAsync(product)).ReturnsAsync(product);

            // Act
            var result = await _productService.CreateProductAsync(product);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
        }

        [Fact]
        public async Task UpdateProduct_ShouldUpdateProduct_WhenProductExists()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1", Price = 10.0m };
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);
            product.Name = "Updated Product 1";

            // Act
            await _productService.UpdateProductAsync(product);

            // Assert
            _mockProductRepository.Verify(repo => repo.Update(product), Times.Once);
        }

        [Fact]
        public async Task DeleteProduct_ShouldRemoveProduct_WhenProductExists()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Product 1", Price = 10.0m };
            _mockProductRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            await _productService.DeleteProductAsync(1);

            // Assert
            _mockProductRepository.Verify(repo => repo.Delete(product), Times.Once);
        }
    }
}