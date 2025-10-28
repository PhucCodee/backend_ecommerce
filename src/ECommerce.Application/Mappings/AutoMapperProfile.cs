using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Mapping between User entity and UserDto
            CreateMap<User, UserDto>().ReverseMap();

            // Mapping between Product entity and ProductDto
            CreateMap<Product, ProductDto>().ReverseMap();

            // Mapping between Order entity and OrderDto
            CreateMap<Order, OrderDto>().ReverseMap();

            // Mapping between OrderItem entity and OrderItemDto
            CreateMap<OrderItem, OrderItemDto>().ReverseMap();
        }
    }
}