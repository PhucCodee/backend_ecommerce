using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace ECommerce.Application.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordService passwordService) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IJwtService _jwtService = jwtService;
        private readonly IPasswordService _passwordService = passwordService;

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            NormalizeDto(registerDto);
            await EnsureEmailAndUsernameAreUnique(registerDto.Email, registerDto.Username);

            var (passwordHash, passwordSalt) = _passwordService.HashPassword(registerDto.Password);

            var user = User.CreateDefault(registerDto.Email, registerDto.Username);
            user.UserCredential = UserCredential.CreateDefault(user, passwordHash, passwordSalt);
            user.UserProfile = UserProfile.CreateDefault(user, registerDto.FirstName, registerDto.LastName, registerDto.Phone);

            var (accessToken, refreshToken, session) = GenerateTokensAndSession(user);
            user.UserSessions.Add(session);

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

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
            NormalizeDto(loginDto);

            var user = await _userRepository.GetByEmailOrUsernameAsync(loginDto.Identifier);

            if (user == null || user.UserCredential == null)
                throw new UnauthorizedException("Invalid credentials");

            if (user.UserProfile == null)
                throw new BadRequestException("User profile not found. Please contact support.");

            if (user.Status != Domain.Enums.UserStatus.active)
                throw new UnauthorizedException("Invalid credentials");

            if (user.UserCredential.LockedUntil.HasValue && user.UserCredential.LockedUntil > DateTime.UtcNow)
                throw new UnauthorizedException("Account is temporarily locked");

            if (!_passwordService.VerifyPassword(loginDto.Password, user.UserCredential.PasswordHash, user.UserCredential.PasswordSalt))
            {
                user.UserCredential.FailedLoginAttempts++;
                if (user.UserCredential.FailedLoginAttempts >= 5)
                    user.UserCredential.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                user.UserCredential.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                throw new UnauthorizedException("Invalid credentials");
            }

            user.UserCredential.FailedLoginAttempts = 0;
            user.UserCredential.LockedUntil = null;
            user.UserCredential.LastLoginAt = DateTime.UtcNow;
            user.UserCredential.UpdatedAt = DateTime.UtcNow;

            var (accessToken, refreshToken, session) = GenerateTokensAndSession(user);
            user.UserSessions.Add(session);

            await _unitOfWork.SaveChangesAsync();

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

        private (string accessToken, string refreshToken, UserSession session) GenerateTokensAndSession(User user)
        {
            var roles = Array.Empty<string>();
            var accessToken = _jwtService.GenerateAccessToken(user.UserId, user.Email, roles);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var session = UserSession.CreateDefault(
                user,
                _passwordService.HashToken(accessToken),
                _passwordService.HashToken(refreshToken)
            );
            return (accessToken, refreshToken, session);
        }

        private async Task EnsureEmailAndUsernameAreUnique(string email, string username)
        {
            if (await _userRepository.EmailExistsAsync(email))
                throw new ConflictException("Email is already registered");
            if (await _userRepository.UsernameExistsAsync(username))
                throw new ConflictException("Username is already taken");
        }

        private static void NormalizeDto(RegisterDto dto)
        {
            dto.Email = dto.Email.Trim().ToLowerInvariant();
            dto.Username = dto.Username.Trim();
            dto.FirstName = dto.FirstName.Trim();
            dto.LastName = dto.LastName.Trim();
            if (dto.Phone != null) dto.Phone = dto.Phone.Trim();
        }

        private static void NormalizeDto(LoginDto dto)
        {
            dto.Identifier = dto.Identifier.Trim().ToLowerInvariant();
        }
    }
}