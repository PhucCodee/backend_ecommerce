using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class AuthService(ApplicationDbContext context, IConfiguration configuration) : IAuthService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Trim and normalize inputs
            registerDto.Email = registerDto.Email.Trim().ToLowerInvariant();
            registerDto.Username = registerDto.Username.Trim();
            registerDto.FirstName = registerDto.FirstName.Trim();
            registerDto.LastName = registerDto.LastName.Trim();

            // Check if email already exists
            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == registerDto.Email);

            if (emailExists)
                throw new ConflictException("Email is already registered");

            // Check if username already exists (case-insensitive)
            var usernameExists = await _context.Users
                .AnyAsync(u => EF.Functions.ILike(u.Username, registerDto.Username));

            if (usernameExists)
                throw new ConflictException("Username is already taken");

            // Hash password
            var (passwordHash, passwordSalt) = SecurityHelper.HashPassword(registerDto.Password);

            // Create user and related entities using factory methods
            var user = User.CreateDefault(registerDto.Email, registerDto.Username);
            user.UserCredential = UserCredential.CreateDefault(user, passwordHash, passwordSalt);
            user.UserProfile = UserProfile.CreateDefault(user, registerDto.FirstName, registerDto.LastName);

            if (!string.IsNullOrWhiteSpace(registerDto.Phone))
            {
                user.UserProfile.Phone = registerDto.Phone.Trim();
            }

            var accessToken = JwtHelper.GenerateAccessToken(user, _configuration);
            var refreshToken = SecurityHelper.GenerateRefreshToken();

            var session = UserSession.CreateDefault(
                user,
                SecurityHelper.HashToken(accessToken),
                SecurityHelper.HashToken(refreshToken)
            );
            user.UserSessions.Add(session);

            // Add the user
            _context.Users.Add(user);

            // Save everything in one transaction
            await _context.SaveChangesAsync();

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
                    FirstName = user.UserProfile.FirstName,
                    LastName = user.UserProfile.LastName,
                    Phone = user.UserProfile.Phone
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Find user
            var user = await _context.Users
                .Include(u => u.UserCredential)
                .Include(u => u.UserProfile)
                .Include(u => u.UserSessions)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email || u.Username == loginDto.Email);

            if (user == null || user.UserCredential == null)
                throw new UnauthorizedException("Invalid credentials");

            if (user.UserProfile == null)
                throw new BadRequestException("User profile not found. Please contact support.");

            // Check account status
            if (user.Status != UserStatus.active)
                throw new UnauthorizedException("Invalid credentials");

            // Check if account is locked
            if (user.UserCredential.LockedUntil.HasValue && user.UserCredential.LockedUntil > DateTime.UtcNow)
                throw new UnauthorizedException("Account is temporarily locked");

            // Verify password
            if (!SecurityHelper.VerifyPassword(loginDto.Password, user.UserCredential.PasswordHash, user.UserCredential.PasswordSalt))
            {
                // Increment failed attempts
                user.UserCredential.FailedLoginAttempts++;
                if (user.UserCredential.FailedLoginAttempts >= 5)
                {
                    user.UserCredential.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                }
                user.UserCredential.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                throw new UnauthorizedException("Invalid credentials");
            }

            // Reset failed attempts on successful login
            user.UserCredential.FailedLoginAttempts = 0;
            user.UserCredential.LockedUntil = null;
            user.UserCredential.LastLoginAt = DateTime.UtcNow;
            user.UserCredential.UpdatedAt = DateTime.UtcNow;

            // Generate tokens
            var accessToken = JwtHelper.GenerateAccessToken(user, _configuration);
            var refreshToken = SecurityHelper.GenerateRefreshToken();

            // Create session
            var session = UserSession.CreateDefault(
                user,
                SecurityHelper.HashToken(accessToken),
                SecurityHelper.HashToken(refreshToken)
            );
            user.UserSessions.Add(session);

            await _context.SaveChangesAsync();

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
                    FirstName = user.UserProfile.FirstName,
                    LastName = user.UserProfile.LastName,
                    Phone = user.UserProfile.Phone
                }
            };
        }

        public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken) => throw new NotImplementedException();
        public Task<AuthOperationResultDto> LogoutAsync(string accessToken) => throw new NotImplementedException();
        public Task<AuthOperationResultDto> ChangePasswordAsync(int userId, string currentPassword, string newPassword) => throw new NotImplementedException();
        public Task<AuthOperationResultDto> ResetPasswordAsync(string email) => throw new NotImplementedException();
        public Task<AuthOperationResultDto> ConfirmEmailAsync(string token, string email) => throw new NotImplementedException();

    }
}