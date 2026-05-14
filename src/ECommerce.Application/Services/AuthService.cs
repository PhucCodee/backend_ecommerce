using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Services;
using MassTransit;

namespace ECommerce.Application.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService,
        IMapper mapper,
        UserValidationHelper validationHelper,
        IEventPublisher eventPublisher
    ) : IAuthService
    {
        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            DtoNormalizer.Normalize(registerDto);
            await validationHelper.EnsureEmailAndUsernameAreUniqueAsync(
                registerDto.Email,
                registerDto.Username
            );

            var passwordHash = passwordService.HashPassword(registerDto.Password);

            var user = User.CreateDefault(registerDto.Email, registerDto.Username);
            user.UserCredential = UserCredential.CreateDefault(user, passwordHash);
            user.UserProfile = UserProfile.CreateDefault(
                user,
                registerDto.FirstName,
                registerDto.LastName,
                registerDto.Phone
            );

            await userRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            await eventPublisher.PublishAsync(
                new UserRegisteredEvent
                {
                    UserId = user.UserId,
                    Email = user.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                }
            );

            var roles = GetActiveRoleNames(user);
            var (accessToken, refreshToken, session) = GenerateTokensAndSession(user, roles);
            user.UserSessions.Add(session);

            await unitOfWork.SaveChangesAsync();
            return CreateAuthResponse(accessToken, refreshToken, user);
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

            if (
                user.UserCredential.LockedUntil.HasValue
                && user.UserCredential.LockedUntil > DateTime.UtcNow
            )
                throw new UnauthorizedException("Account is temporarily locked");

            if (
                !passwordService.VerifyPassword(loginDto.Password, user.UserCredential.PasswordHash)
            )
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

            var roles = GetActiveRoleNames(user);
            var (accessToken, refreshToken, session) = GenerateTokensAndSession(user, roles);
            user.UserSessions.Add(session);

            await unitOfWork.SaveChangesAsync();

            return CreateAuthResponse(accessToken, refreshToken, user);
        }

        public Task<AuthResponseDto> RefreshTokenAsync(string refreshToken) =>
            throw new NotImplementedException();

        public Task<AuthOperationResultDto> LogoutAsync(string accessToken) =>
            throw new NotImplementedException();

        public async Task<AuthOperationResultDto> ChangePasswordAsync(
            int userId,
            string currentPassword,
            string newPassword
        )
        {
            if (
                string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword)
            )
                throw new BadRequestException("Current password and new password are required.");

            var user = await userRepository.GetUserWithAllDetailsAsync(userId);

            if (user == null || user.UserCredential == null)
                throw new NotFoundException("User not found");

            if (user.Status != UserStatus.active)
                throw new UnauthorizedException("Invalid credentials");

            if (!passwordService.VerifyPassword(currentPassword, user.UserCredential.PasswordHash))
            {
                throw new UnauthorizedException("Current password is incorrect");
            }

            if (passwordService.VerifyPassword(newPassword, user.UserCredential.PasswordHash))
            {
                throw new BadRequestException(
                    "New password must be different from current password"
                );
            }

            var passwordHash = passwordService.HashPassword(newPassword);
            user.UserCredential.PasswordHash = passwordHash;
            user.UserCredential.PasswordUpdatedAt = DateTime.UtcNow;
            user.UserCredential.UpdatedAt = DateTime.UtcNow;
            user.UserCredential.FailedLoginAttempts = 0;
            user.UserCredential.LockedUntil = null;

            user.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync();

            return AuthOperationResultDto.SuccessResult("Password changed successfully");
        }

        public Task<AuthOperationResultDto> ResetPasswordAsync(string email) =>
            throw new NotImplementedException();

        public Task<AuthOperationResultDto> ConfirmEmailAsync(string token, string email) =>
            throw new NotImplementedException();

        private static string[] GetActiveRoleNames(User user) =>
            [.. user.UserRoleUsers.Where(r => r.IsActive()).Select(r => r.Role.ToString())];

        private AuthResponseDto CreateAuthResponse(
            string accessToken,
            string refreshToken,
            User user
        ) =>
            new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = mapper.Map<UserDto>(user),
            };

        private (
            string accessToken,
            string refreshToken,
            UserSession session
        ) GenerateTokensAndSession(User user, string[] roles)
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
