using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.user;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User mappings
            CreateMap<User, UserProfileDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.FirstName : string.Empty))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.LastName : string.Empty))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.Phone : null))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.DateOfBirth : null))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.UserProfile != null && src.UserProfile.Gender.HasValue ? src.UserProfile.Gender.Value.ToString() : null))
                .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.AvatarUrl : null))
                .ForMember(dest => dest.Bio, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.Bio : null))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.PreferredLanguage.ToString() : null))
                .ForMember(dest => dest.PreferredCurrency, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.PreferredCurrency.ToString() : null))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.Timezone : null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // Product mappings
            CreateMap<Product, ProductDto>();

            // Order mappings
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>();
        }
    }
}