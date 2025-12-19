using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ECommerce.Domain.Repositories;
using ECommerce.Application.Common.Exceptions;

namespace ECommerce.Application.Services
{
    public class UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IPasswordService _passwordService = passwordService;

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

        public async Task<UserProfileDto> GetByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId) ?? throw new NotFoundException("User not found");
            return MapToProfileDto(user);
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

            var (passwordHash, passwordSalt) = _passwordService.HashPassword(createDto.Password);

            var user = new User
            {
                Email = createDto.Email,
                Username = createDto.Username,
                EmailVerified = false,
                Status = UserStatus.active,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var userProfile = new UserProfile
            {
                User = user,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Gender = UserGender.male,
                Phone = createDto.Phone ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var credentials = new UserCredential
            {
                User = user,
                PasswordHash = passwordHash,
                PasswordSalt = string.Empty,
                PasswordUpdatedAt = DateTime.UtcNow,
                LastLoginIp = string.Empty,
                ResetTokenHash = string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user.UserProfile = userProfile;
            user.UserCredential = credentials;

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return MapToProfileDto(user);
        }

        public async Task<UserProfileDto> UpdateAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await _userRepository.GetWithProfileAsync(userId) ?? throw new NotFoundException("User not found");

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
                var (passwordHash, passwordSalt) = _passwordService.HashPassword(updateDto.Password);
                user.UserCredential.PasswordHash = passwordHash;
                user.UserCredential.PasswordSalt = passwordSalt;
                user.UserCredential.PasswordUpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync();
            return MapToProfileDto(user);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            user.SoftDelete();

            await _unitOfWork.SaveChangesAsync();
            return true;
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