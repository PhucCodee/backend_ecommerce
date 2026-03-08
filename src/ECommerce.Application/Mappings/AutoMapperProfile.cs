using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.cart;
using ECommerce.Application.DTOs.category;
using ECommerce.Application.DTOs.product;
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
            // User - UserProfileDto
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
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoleUsers
                    .Where(r => r.RevokedAt == null)
                    .Select(r => (int)r.Role)
                    .ToArray()));

            // ProductImage - ProductImageDto
            CreateMap<ProductImage, ProductImageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ImageId))
                .ForMember(dest => dest.ProductSkuId, opt => opt.MapFrom(src => src.SkuId));

            // Product - ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Username : null))
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).Sku
                        : src.ProductSkus.Any() ? src.ProductSkus.First().Sku : null))
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
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // ProductSku - ProductSkuDto
            CreateMap<ProductSku, ProductSkuDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : null))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.Inventory != null ? src.Inventory.QuantityAvailable : 0))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                    src.ProductImages.Where(pi => !pi.IsDeleted).OrderBy(pi => pi.DisplayOrder).ToList()));

            // Product - ProductSummaryDto
            CreateMap<Product, ProductSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.VariantCount, opt => opt.MapFrom(src =>
                    src.ProductSkus.Count(ps => !ps.IsDefault)))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).Price
                        : src.ProductSkus.Any() ? src.ProductSkus.First().Price : 0))
                .ForMember(dest => dest.CompareAtPrice, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).CompareAtPrice
                        : null))
                .ForMember(dest => dest.InStock, opt => opt.MapFrom(src =>
                    src.ProductSkus.Any(ps => ps.IsDefault && ps.Inventory != null && ps.Inventory.QuantityAvailable > 0)))
                .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src =>
                    src.ProductSkus.Where(ps => ps.IsDefault)
                        .SelectMany(ps => ps.ProductImages)
                        .Where(i => !i.IsDeleted && i.IsPrimary)
                        .Select(i => i.ThumbnailUrl)
                        .FirstOrDefault()));

            // Product — ProductDetailDto
            CreateMap<Product, ProductDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : null))
                .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Username : null))
                .ForMember(dest => dest.VariantCount, opt => opt.MapFrom(src =>
                    src.ProductSkus.Count(ps => !ps.IsDefault)))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).Price
                        : src.ProductSkus.Any() ? src.ProductSkus.First().Price : 0))
                .ForMember(dest => dest.CompareAtPrice, opt => opt.MapFrom(src =>
                    src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                        ? src.ProductSkus.First(ps => ps.IsDefault).CompareAtPrice
                        : null))
                .ForMember(dest => dest.InStock, opt => opt.MapFrom(src =>
                    src.ProductSkus.Any(ps => ps.IsDefault && ps.Inventory != null && ps.Inventory.QuantityAvailable > 0)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                    src.ProductSkus.Where(s => s.IsDefault)
                        .SelectMany(s => s.ProductImages)
                        .Where(i => !i.IsDeleted)
                        .OrderBy(i => i.DisplayOrder)));

            // Cart - CartDto
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => src.Sku != null ? src.Sku.Sku : null))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Sku != null && src.Sku.Product != null ? src.Sku.Product.ProductName : null))
                .ForMember(dest => dest.VariantAttributes, opt => opt.MapFrom(src => src.Sku != null ? src.Sku.VariantAttributes : null))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                    src.Sku != null
                        ? src.Sku.ProductImages
                            .Where(pi => !pi.IsDeleted)
                            .OrderByDescending(pi => pi.IsPrimary)
                            .ThenBy(pi => pi.DisplayOrder)
                            .Select(pi => pi.ImageUrl)
                            .FirstOrDefault()
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