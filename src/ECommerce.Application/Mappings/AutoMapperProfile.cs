using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.cart;
using ECommerce.Application.DTOs.category;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.DTOs.productsku;
using ECommerce.Application.DTOs.user;
using ECommerce.Domain.Entities;
using System;
using System.Linq;

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
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoleUsers != null && src.UserRoleUsers.Any() 
                    ? src.UserRoleUsers.Where(uru => uru.RevokedAt == null).Select(uru => (int)uru.Role).ToArray() 
                    : new int[0]));

            // ProductImage mapping
            CreateMap<ProductImage, ProductImageDto>();

            // Product mappings
            CreateMap<Product, ProductDto>();
            CreateMap<Product, ProductDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Username : null))
                // SKU from default SKU
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).Sku
                        : src.ProductSkus.Any() ? src.ProductSkus.First().Sku : null))
                // IsDefault - true if this product has a default SKU (primary product)
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src =>
                    src.ProductSkus.Any(ps => ps.IsDefault)))
                // VariantCount - number of non-default SKUs
                .ForMember(dest => dest.VariantCount, opt => opt.MapFrom(src =>
                    src.ProductSkus.Count(ps => !ps.IsDefault)))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).Price
                        : src.ProductSkus.Any() ? src.ProductSkus.First().Price : 0))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null && src.ProductSkus.First(ps => ps.IsDefault).Inventory != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).Inventory!.QuantityAvailable
                        : src.ProductSkus.Any() && src.ProductSkus.First().Inventory != null
                            ? src.ProductSkus.First().Inventory!.QuantityAvailable : 0))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ProductImages.FirstOrDefault(pi => pi.IsPrimary) != null
                        ? src.ProductImages.First(pi => pi.IsPrimary).ImageUrl
                        : src.ProductImages.Any() ? src.ProductImages.First().ImageUrl : null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                    src.ProductImages.OrderBy(pi => pi.DisplayOrder).ToList()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // ProductSku mappings
            CreateMap<ProductSku, ProductSkuDetailDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : null))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.QuantityAvailable : 0))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.ProductImages.Any() ? src.ProductImages.First().ImageUrl : null));

            // Cart mappings
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku != null ? src.Sku.Sku : null))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Sku != null && src.Sku.Product != null ? src.Sku.Product.ProductName : null))
                .ForMember(dest => dest.VariantAttributes, opt => opt.MapFrom(src => src.Sku != null ? src.Sku.VariantAttributes : null))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.Sku != null && src.Sku.ProductImages.Any()
                        ? src.Sku.ProductImages.First().ImageUrl
                        : null))
                .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => src.Sku != null ? src.Sku.Price : 0))
                .ForMember(dest => dest.AvailableStock, opt => opt.MapFrom(src =>
                    src.Sku != null && src.Sku.Inventory != null ? src.Sku.Inventory.QuantityAvailable : 0))
                .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.PriceSnapshot * src.Quantity));

            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentCategoryName, opt => opt.MapFrom(src => src.ParentCategory != null ? src.ParentCategory.CategoryName : null));

            // Order mappings
            CreateMap<Order, OrderDto>();
            CreateMap<OrderItem, OrderItemDto>();
        }
    }
}