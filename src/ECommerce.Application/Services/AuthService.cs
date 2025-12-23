using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService,
        UserValidationHelper validationHelper) : IAuthService
    {
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            DtoNormalizer.Normalize(registerDto);
            await validationHelper.EnsureEmailAndUsernameAreUniqueAsync(registerDto.Email, registerDto.Username);

            var (passwordHash, passwordSalt) = passwordService.HashPassword(registerDto.Password);

            var user = User.CreateDefault(registerDto.Email, registerDto.Username);
            user.UserCredential = UserCredential.CreateDefault(user, passwordHash, passwordSalt);
            user.UserProfile = UserProfile.CreateDefault(user, registerDto.FirstName, registerDto.LastName, registerDto.Phone);

            // Assign default role (buyer)
            var defaultRole = UserRole.CreateDefault(user, UserRoleType.buyer);
            user.UserRoleUsers.Add(defaultRole);

            var roles = user.UserRoleUsers
                .Where(r => r.IsActive())
                .Select(r => r.Role.ToString())
                .ToArray();

            var (accessToken, refreshToken, session) = GenerateTokensAndSession(user, roles);
            user.UserSessions.Add(session);

            await userRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

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
            DtoNormalizer.Normalize(loginDto);

            var user = await userRepository.GetByEmailOrUsernameAsync(loginDto.Identifier);

            if (user == null || user.UserCredential == null)
                throw new UnauthorizedException("Invalid credentials");

            if (user.UserProfile == null)
                throw new BadRequestException("User profile not found. Please contact support.");

            if (user.Status != UserStatus.active)
                throw new UnauthorizedException("Invalid credentials");

            if (user.UserCredential.LockedUntil.HasValue && user.UserCredential.LockedUntil > DateTime.UtcNow)
                throw new UnauthorizedException("Account is temporarily locked");

            if (!passwordService.VerifyPassword(loginDto.Password, user.UserCredential.PasswordHash, user.UserCredential.PasswordSalt))
            {
                user.UserCredential.FailedLoginAttempts++;
                if (user.UserCredential.FailedLoginAttempts >= 5)
                    user.UserCredential.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                user.UserCredential.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.SaveChangesAsync();

                throw new UnauthorizedException("Invalid credentials");
            }

            user.UserCredential.FailedLoginAttempts = 0;
            user.UserCredential.LockedUntil = null;
            user.UserCredential.LastLoginAt = DateTime.UtcNow;
            user.UserCredential.UpdatedAt = DateTime.UtcNow;

            var roles = user.UserRoleUsers
                .Where(r => r.IsActive())
                .Select(r => r.Role.ToString())
                .ToArray();

            var (accessToken, refreshToken, session) = GenerateTokensAndSession(user, roles);
            user.UserSessions.Add(session);

            await unitOfWork.SaveChangesAsync();

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

        private (string accessToken, string refreshToken, UserSession session) GenerateTokensAndSession(User user, string[] roles)
        {
            var accessToken = jwtService.GenerateAccessToken(user.UserId, user.Email, roles);
            var refreshToken = jwtService.GenerateRefreshToken();
            var session = UserSession.CreateDefault(
                user,
                passwordService.HashToken(accessToken),
                passwordService.HashToken(refreshToken)
            );
            return (accessToken, refreshToken, session);
        }
    }
}