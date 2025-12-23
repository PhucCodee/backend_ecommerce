using ECommerce.Application.DTOs.user;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Helpers;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Common.Pagination;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using AutoMapper;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ECommerce.Application.Services
{
    public class UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IMapper mapper,
        UserValidationHelper validationHelper) : IUserService
    {
        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await userRepository.GetUserWithProfileAsync(userId)
                ?? throw new NotFoundException("User not found");
            return mapper.Map<UserProfileDto>(user);
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllAsync()
        {
            var users = await userRepository.GetAllWithProfileAsync();
            return mapper.Map<IEnumerable<UserProfileDto>>(users);
        }

        public async Task<PagedResult<UserProfileDto>> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var (users, totalCount) = await userRepository.GetPagedAsync(paginationParams.PageNumber, paginationParams.PageSize);
            var userDtos = mapper.Map<IEnumerable<UserProfileDto>>(users);
            return PagedResult<UserProfileDto>.Create(userDtos, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
        }

        public async Task<UserProfileDto> GetByIdAsync(int userId)
        {
            var user = await userRepository.GetUserWithProfileAsync(userId)
                ?? throw new NotFoundException("User not found");
            return mapper.Map<UserProfileDto>(user);
        }

        public async Task<UserProfileDto> CreateAsync(UserCreateDto createDto)
        {
            DtoNormalizer.Normalize(createDto);
            await validationHelper.EnsureEmailAndUsernameAreUniqueAsync(createDto.Email, createDto.Username);

            var (passwordHash, passwordSalt) = passwordService.HashPassword(createDto.Password);

            var user = User.CreateDefault(createDto.Email, createDto.Username);
            user.UserCredential = UserCredential.CreateDefault(user, passwordHash, passwordSalt);
            user.UserProfile = UserProfile.CreateDefault(user, createDto.FirstName, createDto.LastName, createDto.Phone);

            // Assign default role (buyer)
            var defaultRole = UserRole.CreateDefault(user, UserRoleType.buyer);
            user.UserRoleUsers.Add(defaultRole);

            await userRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<UserProfileDto>(user);
        }

        public async Task<UserProfileDto> UpdateAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await userRepository.GetWithProfileAsync(userId)
                ?? throw new NotFoundException("User not found");

            DtoNormalizer.Normalize(updateDto);
            await UpdateEmailIfChanged(user, updateDto.Email);
            await UpdateUsernameIfChanged(user, updateDto.Username);
            UpdateProfile(user, updateDto);
            UpdatePasswordIfProvided(user, updateDto.Password);

            user.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<UserProfileDto>(user);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UserUpdateDto updateDto)
        {
            var user = await userRepository.GetWithProfileAsync(userId)
                ?? throw new NotFoundException("User not found");

            DtoNormalizer.Normalize(updateDto);

            // Users can only update their profile info, not email/username
            UpdateProfile(user, updateDto);
            UpdatePasswordIfProvided(user, updateDto.Password);

            user.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<UserProfileDto>(user);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            user.SoftDelete();
            await unitOfWork.SaveChangesAsync();

            return true;
        }

        private async Task UpdateEmailIfChanged(User user, string? newEmail)
        {
            if (string.IsNullOrWhiteSpace(newEmail) || string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
                return;

            await validationHelper.EnsureEmailIsUniqueAsync(newEmail);
            user.Email = newEmail;
        }

        private async Task UpdateUsernameIfChanged(User user, string? newUsername)
        {
            if (string.IsNullOrWhiteSpace(newUsername) || string.Equals(user.Username, newUsername, StringComparison.OrdinalIgnoreCase))
                return;

            await validationHelper.EnsureUsernameIsUniqueAsync(newUsername);
            user.Username = newUsername;
        }

        private static void UpdateProfile(User user, UserUpdateDto updateDto)
        {
            if (user.UserProfile == null) return;

            if (updateDto.FirstName != null)
                user.UserProfile.FirstName = updateDto.FirstName;
            if (updateDto.LastName != null)
                user.UserProfile.LastName = updateDto.LastName;
            if (updateDto.Phone != null)
                user.UserProfile.Phone = updateDto.Phone;
            if (updateDto.DateOfBirth.HasValue)
                user.UserProfile.DateOfBirth = updateDto.DateOfBirth;
            if (updateDto.Gender != null && Enum.TryParse<UserGender>(updateDto.Gender, true, out var gender))
                user.UserProfile.Gender = gender;
            if (updateDto.AvatarUrl != null)
                user.UserProfile.AvatarUrl = updateDto.AvatarUrl;
            if (updateDto.Bio != null)
                user.UserProfile.Bio = updateDto.Bio;
            if (updateDto.PreferredLanguage != null && Enum.TryParse<Language>(updateDto.PreferredLanguage, true, out var lang))
                user.UserProfile.PreferredLanguage = lang;
            if (updateDto.PreferredCurrency != null && Enum.TryParse<Currency>(updateDto.PreferredCurrency, true, out var currency))
                user.UserProfile.PreferredCurrency = currency;
            if (updateDto.Timezone != null)
                user.UserProfile.Timezone = updateDto.Timezone;

            user.UserProfile.UpdatedAt = DateTime.UtcNow;
        }

        private void UpdatePasswordIfProvided(User user, string? newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || user.UserCredential == null)
                return;

            var (passwordHash, passwordSalt) = passwordService.HashPassword(newPassword);
            user.UserCredential.PasswordHash = passwordHash;
            user.UserCredential.PasswordSalt = passwordSalt;
            user.UserCredential.PasswordUpdatedAt = DateTime.UtcNow;
            user.UserCredential.UpdatedAt = DateTime.UtcNow;
        }
    }
}