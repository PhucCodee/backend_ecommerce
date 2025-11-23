using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using System;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IJwtService jwtService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPasswordService _passwordService = passwordService;
        private readonly IJwtService _jwtService = jwtService;

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Validate if user already exists
            if (await _userRepository.EmailExistsAsync(registerDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            if (await _userRepository.UsernameExistsAsync(registerDto.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            // Hash password (BCrypt includes salt automatically)
            var hash = _passwordService.HashPassword(registerDto.Password);

            // Create user
            var user = new User
            {
                Email = registerDto.Email,
                Username = registerDto.Username,
                EmailVerified = false,
                Status = UserStatus.active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user profile
            var userProfile = new UserProfile
            {
                User = user,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Gender = UserGender.male,
                Phone = registerDto.Phone ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create user credentials
            var userCredential = new UserCredential
            {
                User = user,
                PasswordHash = hash,
                PasswordSalt = string.Empty, // BCrypt doesn't need separate salt
                PasswordUpdatedAt = DateTime.UtcNow,
                LastLoginIp = string.Empty,
                ResetTokenHash = string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Set navigation properties
            user.UserProfile = userProfile;
            user.UserCredential = userCredential;

            // Save to database
            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user.UserId, user.Email, ["User"]);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Token expires in 1 hour
                User = new UserProfileDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = userProfile.FirstName,
                    LastName = userProfile.LastName,
                    Phone = userProfile.Phone,
                    EmailVerified = user.EmailVerified,
                    Status = user.Status.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Get user with credentials
            var user = await _userRepository.GetUserWithCredentialsAsync(loginDto.Email);
            if (user == null || user.UserCredential == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password (BCrypt handles salt automatically)
            if (!_passwordService.VerifyPassword(loginDto.Password, user.UserCredential.PasswordHash))
            {
                // Update failed login attempts
                user.UserCredential.FailedLoginAttempts++;
                user.UserCredential.LastFailedAttemptAt = DateTime.UtcNow;

                if (user.UserCredential.FailedLoginAttempts >= 5)
                {
                    user.UserCredential.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                }

                await _unitOfWork.SaveChangesAsync();
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Check if account is locked
            if (user.UserCredential.LockedUntil.HasValue && user.UserCredential.LockedUntil > DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Account is temporarily locked due to multiple failed login attempts");
            }

            // Reset failed login attempts on successful login
            user.UserCredential.FailedLoginAttempts = 0;
            user.UserCredential.LastFailedAttemptAt = null;
            user.UserCredential.LastLoginAt = DateTime.UtcNow;
            user.UserCredential.LastLoginIp = "127.0.0.1"; // Should get from request context
            user.UserCredential.LockedUntil = null;

            await _unitOfWork.SaveChangesAsync();

            // Get user profile
            var userWithProfile = await _userRepository.GetUserWithProfileAsync(user.UserId);

            // Generate tokens
            var accessToken = _jwtService.GenerateAccessToken(user.UserId, user.Email, new[] { "User" });
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserProfileDto
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = userWithProfile?.UserProfile?.FirstName ?? "",
                    LastName = userWithProfile?.UserProfile?.LastName ?? "",
                    Phone = userWithProfile?.UserProfile?.Phone ?? "",
                    EmailVerified = user.EmailVerified,
                    Status = user.Status.ToString(),
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetUserWithProfileAsync(userId) ?? throw new InvalidOperationException("User not found");
            return new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.UserProfile?.FirstName ?? "",
                LastName = user.UserProfile?.LastName ?? "",
                Phone = user.UserProfile?.Phone ?? "",
                EmailVerified = user.EmailVerified,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}