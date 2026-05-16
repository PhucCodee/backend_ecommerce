using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.user;

namespace ECommerce.Application.Helpers
{
    public static class DtoNormalizer
    {
        public static void Normalize(RegisterDto dto)
        {
            dto.Email = dto.Email.Trim().ToLowerInvariant();
            dto.Username = dto.Username.Trim();
            dto.FirstName = dto.FirstName.Trim();
            dto.LastName = dto.LastName.Trim();
            if (dto.Phone != null)
                dto.Phone = dto.Phone.Trim();
        }

        public static void Normalize(LoginDto dto)
        {
            dto.Identifier = dto.Identifier.Trim().ToLowerInvariant();
        }

        public static void Normalize(UserCreateDto dto)
        {
            dto.Email = dto.Email.Trim().ToLowerInvariant();
            dto.Username = dto.Username.Trim();
            dto.FirstName = dto.FirstName.Trim();
            dto.LastName = dto.LastName.Trim();
            if (dto.Phone != null)
                dto.Phone = dto.Phone.Trim();
        }

        public static void Normalize(UserUpdateDto dto)
        {
            if (dto.FirstName != null)
                dto.FirstName = dto.FirstName.Trim();
            if (dto.LastName != null)
                dto.LastName = dto.LastName.Trim();
            if (dto.Phone != null)
                dto.Phone = dto.Phone.Trim();
            if (dto.Bio != null)
                dto.Bio = dto.Bio.Trim();
            if (dto.AvatarUrl != null)
                dto.AvatarUrl = dto.AvatarUrl.Trim();
            if (dto.Timezone != null)
                dto.Timezone = dto.Timezone.Trim();
        }
    }
}
