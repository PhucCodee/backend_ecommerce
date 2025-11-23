using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class AuthService(ApplicationDbContext context, IConfiguration configuration) : IAuthService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email || u.Username == registerDto.Username);

            if (existingUser != null)
                throw new InvalidOperationException("User with this email or username already exists");

            // Hash password
            var (passwordHash, passwordSalt) = SecurityHelper.HashPassword(registerDto.Password);

            // Create user and related entities using factory methods
            var user = User.CreateDefault(registerDto.Email, registerDto.Username);
            user.UserCredential = UserCredential.CreateDefault(user, passwordHash, passwordSalt);
            user.UserProfile = UserProfile.CreateDefault(user, registerDto.FirstName, registerDto.LastName);

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
                throw new UnauthorizedAccessException("Invalid credentials");

            // Check account status
            if (user.Status != UserStatus.active)
                throw new UnauthorizedAccessException("Account is not active");

            // Check if account is locked
            if (user.UserCredential.LockedUntil.HasValue && user.UserCredential.LockedUntil > DateTime.UtcNow)
                throw new UnauthorizedAccessException("Account is temporarily locked");

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

                throw new UnauthorizedAccessException("Invalid credentials");
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

        public Task<bool> RefreshTokenAsync(string refreshToken) => throw new NotImplementedException();
        public Task<bool> LogoutAsync(string accessToken) => throw new NotImplementedException();
        public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword) => throw new NotImplementedException();
        public Task<bool> ResetPasswordAsync(string email) => throw new NotImplementedException();
        public Task<bool> ConfirmEmailAsync(string token, string email) => throw new NotImplementedException();
    }
}