using System;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Infrastructure.Services;

namespace ECommerce.UnitTests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IPasswordService> _passwordServiceMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _passwordServiceMock = new Mock<IPasswordService>();
            _jwtServiceMock = new Mock<IJwtService>();
            _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            _userService = new UserService(
                _userRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _passwordServiceMock.Object,
                _jwtServiceMock.Object
            );
        }

        [Fact]
        public async Task GetProfileAsync_Returns_Profile_When_UserExists()
        {
            // Arrange
            var user = new User
            {
                UserId = 1,
                Email = "test@example.com",
                Username = "tester",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserProfile = new UserProfile
                {
                    FirstName = "Test",
                    LastName = "User",
                    Phone = "123",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            _userRepositoryMock.Setup(r => r.GetUserWithProfileAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetProfileAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("tester", result.Username);
            Assert.Equal("Test", result.FirstName);
        }

        [Fact]
        public async Task CreateAsync_ShouldHashPassword_AndSaveUser()
        {
            // Arrange
            var createDto = new UserCreateDto
            {
                Email = "new@example.com",
                Username = "newuser",
                Password = "Password123",
                FirstName = "New",
                LastName = "User"
            };
            _passwordServiceMock.Setup(p => p.HashPassword(createDto.Password)).Returns("hashed");
            _userRepositoryMock.Setup(r => r.EmailExistsAsync(createDto.Email)).ReturnsAsync(false);
            _userRepositoryMock.Setup(r => r.UsernameExistsAsync(createDto.Username)).ReturnsAsync(false);
            _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);

            // Act
            var result = await _userService.CreateAsync(createDto);

            // Assert
            Assert.Equal(createDto.Email, result.Email);
            _passwordServiceMock.Verify(p => p.HashPassword(createDto.Password), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }
    }
}