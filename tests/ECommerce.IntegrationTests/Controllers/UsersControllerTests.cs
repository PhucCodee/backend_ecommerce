using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using ECommerce.API.Controllers;
using ECommerce.Application.Interfaces;
using ECommerce.Application.DTOs;
using Moq;

namespace ECommerce.IntegrationTests.Controllers
{
    public class UsersControllerTests
    {
        private readonly UsersController _controller;
        private readonly Mock<IUserService> _userServiceMock;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _controller = new UsersController(_userServiceMock.Object);
        }

        [Fact]
        public async Task Register_User_Returns_Created()
        {
            // Arrange
            var userDto = new UserDto { Email = "test@example.com", Password = "Password123" };
            _userServiceMock.Setup(service => service.RegisterAsync(userDto)).ReturnsAsync(true);

            // Act
            var result = await _controller.Register(userDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult>(result);
            Assert.Equal((int)HttpStatusCode.Created, actionResult.Result.StatusCode);
        }

        [Fact]
        public async Task Login_User_Returns_OK()
        {
            // Arrange
            var loginDto = new UserDto { Email = "test@example.com", Password = "Password123" };
            _userServiceMock.Setup(service => service.LoginAsync(loginDto)).ReturnsAsync("token");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var actionResult = Assert.IsType<ActionResult>(result);
            Assert.Equal((int)HttpStatusCode.OK, actionResult.Result.StatusCode);
        }

        // Additional tests for other endpoints can be added here
    }
}