using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.user;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace ECommerce.Application.Services
{
    public class UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IMapper mapper,
        UserValidationHelper validationHelper
    ) : IUserService
    {
        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user =
                await userRepository.GetUserWithProfileAsync(userId)
                ?? throw new NotFoundException("User not found");
            return mapper.Map<UserProfileDto>(user);
        }

        public async Task<PagedResult<UserDto>> GetAllPagedAsync(PaginationParams paginationParams)
        {
            var (users, totalCount) = await userRepository.GetPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize
            );
            var userDtos = mapper.Map<IEnumerable<UserDto>>(users);
            return PagedResult<UserDto>.Create(
                userDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount
            );
        }

        public async Task<UserDto> GetByIdAsync(int userId)
        {
            var user =
                await userRepository.GetUserWithProfileAsync(userId)
                ?? throw new NotFoundException("User not found");
            return mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> CreateAsync(UserCreateDto createDto)
        {
            DtoNormalizer.Normalize(createDto);
            await validationHelper.EnsureEmailAndUsernameAreUniqueAsync(
                createDto.Email,
                createDto.Username
            );

            var passwordHash = passwordService.HashPassword(createDto.Password);

            var user = User.CreateDefault(createDto.Email, createDto.Username);
            user.UserCredential = UserCredential.CreateDefault(user, passwordHash);
            user.UserProfile = UserProfile.CreateDefault(
                user,
                createDto.FirstName,
                createDto.LastName,
                createDto.Phone ?? string.Empty
            );

            // Assign roles from DTO
            var rolesToAssign =
                createDto.Roles != null && createDto.Roles.Length > 0
                    ? createDto.Roles.Select(r => (UserRoleType)r).Distinct().ToArray()
                    : [UserRoleType.buyer];

            foreach (var role in rolesToAssign)
            {
                if (Enum.IsDefined(typeof(UserRoleType), role))
                {
                    var userRole = UserRole.CreateDefault(user, role);
                    user.UserRoleUsers.Add(userRole);
                }
            }

            await userRepository.AddAsync(user);
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateAsync(int userId, UserUpdateDto updateDto)
        {
            var user =
                await userRepository.GetUserWithAllDetailsAsync(userId)
                ?? throw new NotFoundException("User not found");

            DtoNormalizer.Normalize(updateDto);
            UpdateProfile(user, updateDto);

            if (updateDto.Roles != null)
            {
                UpdateUserRoles(user, updateDto.Roles);
            }

            user.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<UserDto>(user);
        }

        public async Task<UserProfileDto> UpdateProfileAsync(int userId, UserUpdateDto updateDto)
        {
            var user =
                await userRepository.GetUserWithProfileAsync(userId)
                ?? throw new NotFoundException("User not found");

            DtoNormalizer.Normalize(updateDto);
            UpdateProfile(user, updateDto);

            user.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            return mapper.Map<UserProfileDto>(user);
        }

        public async Task<bool> DeleteAsync(int userId)
        {
            var user =
                await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            if (user.IsDeleted())
                throw new NotFoundException("User not found");

            // Hard delete: actually remove the row from the database. Related
            // rows (credentials, profile, addresses, sessions, roles, carts,
            // reviews, item interactions, the seller's products, ...) are
            // wiped via ON DELETE CASCADE that's already declared in the
            // schema. Orders and order items keep ON DELETE RESTRICT so users
            // who have transacted as buyer or seller can't be erased — that
            // case is converted to a 409 below.
            await userRepository.DeleteAsync(userId);

            try
            {
                await unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
            {
                throw new ConflictException(
                    "Cannot delete this user because they have existing orders or sales records. "
                        + "Deactivate the account instead."
                );
            }

            return true;
        }

        private static bool IsForeignKeyViolation(DbUpdateException ex)
        {
            return ex.InnerException is PostgresException pg
                && pg.SqlState == PostgresErrorCodes.ForeignKeyViolation;
        }

        private static void UpdateProfile(User user, UserUpdateDto updateDto)
        {
            if (user.UserProfile == null)
                return;

            if (updateDto.FirstName != null)
                user.UserProfile.FirstName = updateDto.FirstName;
            if (updateDto.LastName != null)
                user.UserProfile.LastName = updateDto.LastName;
            if (updateDto.Phone != null)
                user.UserProfile.Phone = updateDto.Phone;
            if (updateDto.DateOfBirth.HasValue)
                user.UserProfile.DateOfBirth = updateDto.DateOfBirth;
            if (
                updateDto.Gender != null
                && Enum.TryParse<UserGender>(updateDto.Gender, true, out var gender)
            )
                user.UserProfile.Gender = gender;
            if (updateDto.AvatarUrl != null)
                user.UserProfile.AvatarUrl = updateDto.AvatarUrl;
            if (updateDto.Bio != null)
                user.UserProfile.Bio = updateDto.Bio;
            if (
                updateDto.PreferredLanguage != null
                && Enum.TryParse<Language>(updateDto.PreferredLanguage, true, out var lang)
            )
                user.UserProfile.PreferredLanguage = lang;
            if (
                updateDto.PreferredCurrency != null
                && Enum.TryParse<Currency>(updateDto.PreferredCurrency, true, out var currency)
            )
                user.UserProfile.PreferredCurrency = currency;
            if (updateDto.Timezone != null)
                user.UserProfile.Timezone = updateDto.Timezone;

            user.UserProfile.UpdatedAt = DateTime.UtcNow;
        }

        private static void UpdateUserRoles(User user, int[] roleIds)
        {
            if (roleIds == null || roleIds.Length == 0)
            {
                roleIds = [(int)UserRoleType.buyer];
            }

            var activeRoles = user.UserRoleUsers.Where(r => r.RevokedAt == null).ToList();
            foreach (var role in activeRoles)
            {
                role.RevokedAt = DateTime.UtcNow;
            }

            var rolesToAssign = roleIds
                .Select(r => (UserRoleType)r)
                .Where(r => Enum.IsDefined(typeof(UserRoleType), r))
                .Distinct()
                .ToArray();

            foreach (var roleType in rolesToAssign)
            {
                var existingRole = user.UserRoleUsers.FirstOrDefault(r => r.Role == roleType);
                if (existingRole != null)
                {
                    if (existingRole.RevokedAt != null)
                    {
                        existingRole.RevokedAt = null;
                        existingRole.GrantedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    var newRole = UserRole.CreateDefault(user, roleType);
                    user.UserRoleUsers.Add(newRole);
                }
            }
        }
    }
}
