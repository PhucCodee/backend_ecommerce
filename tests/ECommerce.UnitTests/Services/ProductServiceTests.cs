using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Application.DTOs;

namespace ECommerce.UnitTests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly IProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
            _productService = new ProductService(_mockProductRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public async Task GetAllProducts_ShouldReturnAllProducts()
        {
            // Arrange
            var products = new List<Product>
            {
                CreateProduct(1, 10m, 5),
                CreateProduct(2, 20m, 3)
            };
            _mockProductRepository.Setup(repo => repo.GetAllWithDetailsAsync()).ReturnsAsync(products);

            // Act
            var result = await _productService.GetAllProductsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetProductById_ShouldReturnProduct_WhenProductExists()
        {
            // Arrange
            var product = CreateProduct(1, 10m, 5);
            _mockProductRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result?.Name);
        }

        [Fact]
        public async Task GetProductById_ShouldReturnNull_WhenProductDoesNotExist()
        {
            // Arrange
            _mockProductRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1)).ReturnsAsync((Product)null);

            // Act
            var result = await _productService.GetProductByIdAsync(1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateProduct_ShouldAddProduct()
        {
            // Arrange
            _mockProductRepository.Setup(repo => repo.AddAsync(It.IsAny<Product>()))
                .ReturnsAsync((Product p) => p);
            var dto = new ProductDto { Name = "Product 1", Price = 10m, Stock = 5, CategoryId = 1, SellerId = 1 };

            // Act
            var result = await _productService.CreateProductAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Product 1", result.Name);
        }

        [Fact]
        public async Task UpdateProduct_ShouldUpdateProduct_WhenProductExists()
        {
            // Arrange
            var product = CreateProduct(1, 10m, 5);
            _mockProductRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1)).ReturnsAsync(product);
            var dto = new ProductDto { Name = "Updated Product 1", Price = 15m, Stock = 3, CategoryId = 1, SellerId = 1 };

            // Act
            var result = await _productService.UpdateProductAsync(1, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Product 1", result?.Name);
        }

        [Fact]
        public async Task DeleteProduct_ShouldRemoveProduct_WhenProductExists()
        {
            // Arrange
            _mockProductRepository.Setup(repo => repo.DeleteAsync(1)).Returns(Task.CompletedTask);
            _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await _productService.DeleteProductAsync(1);

            // Assert
            Assert.True(result);
        }

        private static Product CreateProduct(int id, decimal price, int stock)
        {
            var now = DateTime.UtcNow;
            var product = new Product
            {
                ProductId = id,
                ProductName = $"Product {id}",
                CategoryId = 1,
                SellerId = 1,
                Slug = $"product-{id}",
                BaseSku = $"SKU-{id}",
                CreatedAt = now,
                UpdatedAt = now,
            };

            var sku = new ProductSku
            {
                Product = product,
                SkuId = id,
                Sku = $"SKU-{id}",
                VariantAttributes = "default",
                Price = price,
                IsDefault = true,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            };

            sku.Inventory = new Inventory
            {
                Sku = sku,
                QuantityAvailable = stock,
                QuantityReserved = 0,
                QuantitySold = 0,
                ReorderPoint = 0,
                ReorderQuantity = 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            product.ProductSkus.Add(sku);
            return product;
        }
    }
}