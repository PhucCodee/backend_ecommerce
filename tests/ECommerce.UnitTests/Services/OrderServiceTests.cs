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
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly IOrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderService = new OrderService(_orderRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnOrder_WhenOrderIsValid()
        {
            // Arrange
            var order = new Order { Id = 1, /* other properties */ };
            _orderRepositoryMock.Setup(repo => repo.AddAsync(order)).ReturnsAsync(order);

            // Act
            var result = await _orderService.CreateOrderAsync(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.Id, result.Id);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturnOrder_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            var order = new Order { Id = orderId, /* other properties */ };
            _orderRepositoryMock.Setup(repo => repo.GetByIdAsync(orderId)).ReturnsAsync(order);

            // Act
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Id);
        }

        [Fact]
        public async Task GetAllOrders_ShouldReturnListOfOrders()
        {
            // Arrange
            var orders = new List<Order>
            {
                new Order { Id = 1, /* other properties */ },
                new Order { Id = 2, /* other properties */ }
            };
            _orderRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task DeleteOrder_ShouldReturnTrue_WhenOrderExists()
        {
            // Arrange
            var orderId = 1;
            _orderRepositoryMock.Setup(repo => repo.DeleteAsync(orderId)).ReturnsAsync(true);

            // Act
            var result = await _orderService.DeleteOrderAsync(orderId);

            // Assert
            Assert.True(result);
        }
    }
}