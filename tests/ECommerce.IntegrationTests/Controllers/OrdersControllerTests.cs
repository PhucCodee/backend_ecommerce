using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using ECommerce.API.Controllers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.DTOs;

namespace ECommerce.IntegrationTests.Controllers
{
    public class OrdersControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public OrdersControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CreateOrder_ReturnsCreatedStatus()
        {
            // Arrange
            var orderDto = new OrderDto
            {
                // Initialize properties for the order
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/orders", orderDto);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task GetOrder_ReturnsOkStatus()
        {
            // Arrange
            var orderId = 1; // Use a valid order ID

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetOrder_ReturnsNotFoundStatus_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = 999; // Use an invalid order ID

            // Act
            var response = await _client.GetAsync($"/api/orders/{orderId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}