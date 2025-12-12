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
using System.Collections.Generic;
using System.Linq;
using ECommerce.Domain.Repositories;

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

        public async Task<IEnumerable<UserProfileDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllWithProfileAsync();
            return users.Select(MapToProfileDto);
        }

        public async Task<UserProfileDto?> GetByIdAsync(int userId)
        {
            var user = await _userRepository.GetWithProfileAsync(userId);
            return user == null ? null : MapToProfileDto(user);
        }

        public async Task<UserProfileDto> CreateAsync(UserCreateDto createDto)
        {
            if (await _userRepository.EmailExistsAsync(createDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            if (await _userRepository.UsernameExistsAsync(createDto.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }

            var now = DateTime.UtcNow;
            var passwordHash = _passwordService.HashPassword(createDto.Password);

            var user = new User
            {
                Email = createDto.Email,
                Username = createDto.Username,
                EmailVerified = false,
                Status = UserStatus.active,
                CreatedAt = now,
                UpdatedAt = now
            };

            var userProfile = new UserProfile
            {
                User = user,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Gender = UserGender.male,
                Phone = createDto.Phone ?? string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            };

            var credentials = new UserCredential
            {
                User = user,
                PasswordHash = passwordHash,
                PasswordSalt = string.Empty,
                PasswordUpdatedAt = now,
                LastLoginIp = string.Empty,
                ResetTokenHash = string.Empty,
                CreatedAt = now,
                UpdatedAt = now
            };

            user.UserProfile = userProfile;
            user.UserCredential = credentials;

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return MapToProfileDto(user);
        }

        public async Task<UserProfileDto?> UpdateAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await _userRepository.GetWithProfileAsync(userId);
            if (user == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Email) && !string.Equals(user.Email, updateDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.EmailExistsAsync(updateDto.Email))
                {
                    throw new InvalidOperationException("Email already exists");
                }

                user.Email = updateDto.Email;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Username) && !string.Equals(user.Username, updateDto.Username, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.UsernameExistsAsync(updateDto.Username))
                {
                    throw new InvalidOperationException("Username already exists");
                }

                user.Username = updateDto.Username;
            }

            if (user.UserProfile != null)
            {
                user.UserProfile.FirstName = updateDto.FirstName ?? user.UserProfile.FirstName;
                user.UserProfile.LastName = updateDto.LastName ?? user.UserProfile.LastName;
                user.UserProfile.Phone = updateDto.Phone ?? user.UserProfile.Phone;
            }

            if (!string.IsNullOrWhiteSpace(updateDto.Password) && user.UserCredential != null)
            {
                user.UserCredential.PasswordHash = _passwordService.HashPassword(updateDto.Password);
                user.UserCredential.PasswordUpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
            return MapToProfileDto(user);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            await _userRepository.DeleteAsync(userId);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }

        private static UserProfileDto MapToProfileDto(User user)
        {
            return new UserProfileDto
            {
                UserId = user.UserId,
                Email = user.Email,
                Username = user.Username,
                FirstName = user.UserProfile?.FirstName ?? string.Empty,
                LastName = user.UserProfile?.LastName ?? string.Empty,
                Phone = user.UserProfile?.Phone ?? string.Empty,
                EmailVerified = user.EmailVerified,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt
            };
        }
    }
}