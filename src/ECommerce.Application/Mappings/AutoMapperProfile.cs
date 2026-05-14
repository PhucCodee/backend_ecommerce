using System;
using System.Linq;
using AutoMapper;
using ECommerce.Application.DTOs;
using ECommerce.Application.DTOs.address;
using ECommerce.Application.DTOs.cart;
using ECommerce.Application.DTOs.category;
using ECommerce.Application.DTOs.coupon;
using ECommerce.Application.DTOs.inventory;
using ECommerce.Application.DTOs.order;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.DTOs.review;
using ECommerce.Application.DTOs.user;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // User - UserProfileDto
            CreateMap<User, UserProfileDto>()
                .ForMember(
                    dest => dest.FirstName,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null ? src.UserProfile.FirstName : string.Empty
                        )
                )
                .ForMember(
                    dest => dest.LastName,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null ? src.UserProfile.LastName : string.Empty
                        )
                )
                .ForMember(
                    dest => dest.Phone,
                    opt =>
                        opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.Phone : null)
                )
                .ForMember(
                    dest => dest.DateOfBirth,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null ? src.UserProfile.DateOfBirth : null
                        )
                )
                .ForMember(
                    dest => dest.Gender,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null && src.UserProfile.Gender.HasValue
                                ? src.UserProfile.Gender.Value.ToString()
                                : null
                        )
                )
                .ForMember(
                    dest => dest.AvatarUrl,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null ? src.UserProfile.AvatarUrl : null
                        )
                )
                .ForMember(
                    dest => dest.Bio,
                    opt => opt.MapFrom(src => src.UserProfile != null ? src.UserProfile.Bio : null)
                )
                .ForMember(
                    dest => dest.PreferredLanguage,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null
                                ? src.UserProfile.PreferredLanguage.ToString()
                                : null
                        )
                )
                .ForMember(
                    dest => dest.PreferredCurrency,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null
                                ? src.UserProfile.PreferredCurrency.ToString()
                                : null
                        )
                )
                .ForMember(
                    dest => dest.Timezone,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserProfile != null ? src.UserProfile.Timezone : null
                        )
                )
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(
                    dest => dest.Roles,
                    opt =>
                        opt.MapFrom(src =>
                            src.UserRoleUsers.Where(r => r.RevokedAt == null)
                                .Select(r => (int)r.Role)
                                .ToArray()
                        )
                );

            // ProductImage - ProductImageDto
            CreateMap<ProductImage, ProductImageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ImageId))
                .ForMember(dest => dest.ProductSkuId, opt => opt.MapFrom(src => src.SkuId));

            // Product - ProductDto
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(
                    dest => dest.Categories,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductCategories.Select(pc => new CategorySimpleDto
                                {
                                    CategoryId = pc.CategoryId,
                                    CategoryName = pc.Category.CategoryName,
                                    IsPrimary = pc.IsPrimary,
                                })
                                .ToList()
                        )
                )
                .ForMember(
                    dest => dest.SellerName,
                    opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Username : null)
                )
                .ForMember(
                    dest => dest.Sku,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                                ? src.ProductSkus.First(ps => ps.IsDefault).Sku
                            : src.ProductSkus.Any() ? src.ProductSkus.First().Sku
                            : null
                        )
                )
                .ForMember(
                    dest => dest.VariantCount,
                    opt => opt.MapFrom(src => src.ProductSkus.Count(ps => !ps.IsDefault))
                )
                .ForMember(
                    dest => dest.Price,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                                ? src.ProductSkus.First(ps => ps.IsDefault).Price
                            : src.ProductSkus.Any() ? src.ProductSkus.First().Price
                            : 0
                        )
                )
                .ForMember(
                    dest => dest.Stock,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                            && src.ProductSkus.First(ps => ps.IsDefault).Inventory != null
                                ? src
                                    .ProductSkus.First(ps => ps.IsDefault)
                                    .Inventory!.QuantityAvailable
                            : src.ProductSkus.Any() && src.ProductSkus.First().Inventory != null
                                ? src.ProductSkus.First().Inventory!.QuantityAvailable
                            : 0
                        )
                )
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            // ProductSku - ProductSkuDto
            CreateMap<ProductSku, ProductSkuDto>()
                .ForMember(
                    dest => dest.ProductName,
                    opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : null)
                )
                .ForMember(
                    dest => dest.Stock,
                    opt =>
                        opt.MapFrom(src =>
                            src.Inventory != null ? src.Inventory.QuantityAvailable : 0
                        )
                )
                .ForMember(
                    dest => dest.QuantityReserved,
                    opt =>
                        opt.MapFrom(src =>
                            src.Inventory != null ? src.Inventory.QuantityReserved : 0
                        )
                )
                .ForMember(
                    dest => dest.QuantitySold,
                    opt =>
                        opt.MapFrom(src => src.Inventory != null ? src.Inventory.QuantitySold : 0)
                )
                .ForMember(
                    dest => dest.ReorderPoint,
                    opt =>
                        opt.MapFrom(src => src.Inventory != null ? src.Inventory.ReorderPoint : 0)
                )
                .ForMember(
                    dest => dest.ReorderQuantity,
                    opt =>
                        opt.MapFrom(src =>
                            src.Inventory != null ? src.Inventory.ReorderQuantity : 0
                        )
                )
                .ForMember(
                    dest => dest.LastRestockedAt,
                    opt =>
                        opt.MapFrom(src =>
                            src.Inventory != null ? src.Inventory.LastRestockedAt : null
                        )
                )
                .ForMember(
                    dest => dest.Images,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductImages.Where(pi => !pi.IsDeleted)
                                .OrderBy(pi => pi.DisplayOrder)
                                .ToList()
                        )
                );

            // Product - ProductSummaryDto
            CreateMap<Product, ProductSummaryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(
                    dest => dest.PrimaryCategoryName,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductCategories.Where(pc => pc.IsPrimary)
                                .Select(pc => pc.Category.CategoryName)
                                .FirstOrDefault()
                        )
                )
                .ForMember(
                    dest => dest.CategoryNames,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductCategories.Select(pc => pc.Category.CategoryName).ToList()
                        )
                )
                .ForMember(
                    dest => dest.CategoryIds,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductCategories.OrderByDescending(pc => pc.IsPrimary)
                                .Select(pc => pc.CategoryId)
                                .ToList()
                        )
                )
                .ForMember(
                    dest => dest.VariantCount,
                    opt => opt.MapFrom(src => src.ProductSkus.Count(ps => !ps.IsDefault))
                )
                .ForMember(
                    dest => dest.Price,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                                ? src.ProductSkus.First(ps => ps.IsDefault).Price
                            : src.ProductSkus.Any() ? src.ProductSkus.First().Price
                            : 0
                        )
                )
                .ForMember(
                    dest => dest.CompareAtPrice,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                                ? src.ProductSkus.First(ps => ps.IsDefault).CompareAtPrice
                                : null
                        )
                )
                .ForMember(
                    dest => dest.InStock,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.Any(ps =>
                                ps.IsDefault
                                && ps.Inventory != null
                                && ps.Inventory.QuantityAvailable > 0
                            )
                        )
                )
                .ForMember(
                    dest => dest.ThumbnailUrl,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.Where(ps => ps.IsDefault)
                                .SelectMany(ps => ps.ProductImages)
                                .Where(i => !i.IsDeleted && i.IsPrimary)
                                .Select(i => i.ThumbnailUrl)
                                .FirstOrDefault()
                        )
                );

            // Product — ProductDetailDto
            CreateMap<Product, ProductDetailDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.ProductName))
                .ForMember(
                    dest => dest.CategoryName,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductCategories.Where(pc => pc.IsPrimary)
                                .Select(pc => pc.Category.CategoryName)
                                .FirstOrDefault()
                        )
                )
                .ForMember(
                    dest => dest.SellerName,
                    opt => opt.MapFrom(src => src.Seller != null ? src.Seller.Username : null)
                )
                .ForMember(
                    dest => dest.VariantCount,
                    opt => opt.MapFrom(src => src.ProductSkus.Count(ps => !ps.IsDefault))
                )
                .ForMember(
                    dest => dest.AverageRating,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductMetrics.OrderByDescending(m => m.Date)
                                .Select(m => m.AverageRating ?? 0)
                                .FirstOrDefault()
                        )
                )
                .ForMember(
                    dest => dest.ReviewCount,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductMetrics.OrderByDescending(m => m.Date)
                                .Select(m => m.ReviewCount)
                                .FirstOrDefault()
                        )
                )
                .ForMember(
                    dest => dest.Price,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                                ? src.ProductSkus.First(ps => ps.IsDefault).Price
                            : src.ProductSkus.Any() ? src.ProductSkus.First().Price
                            : 0
                        )
                )
                .ForMember(
                    dest => dest.CompareAtPrice,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.FirstOrDefault(ps => ps.IsDefault) != null
                                ? src.ProductSkus.First(ps => ps.IsDefault).CompareAtPrice
                                : null
                        )
                )
                .ForMember(
                    dest => dest.InStock,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.Any(ps =>
                                ps.IsDefault
                                && ps.Inventory != null
                                && ps.Inventory.QuantityAvailable > 0
                            )
                        )
                )
                .ForMember(
                    dest => dest.Images,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.Where(s => s.IsDefault)
                                .SelectMany(s => s.ProductImages)
                                .Where(i => !i.IsDeleted)
                                .OrderBy(i => i.DisplayOrder)
                        )
                )
                .ForMember(
                    dest => dest.Skus,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductSkus.Where(s => s.IsActive)
                                .OrderByDescending(s => s.IsDefault)
                                .ThenBy(s => s.CreatedAt)
                        )
                );

            // Cart - CartDto
            CreateMap<Cart, CartDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.CartItems));

            CreateMap<CartItem, CartItemDto>()
                .ForMember(
                    dest => dest.SellerId,
                    opt =>
                        opt.MapFrom(src =>
                            src.Sku != null && src.Sku.Product != null
                                ? src.Sku.Product.SellerId
                                : 0
                        )
                )
                .ForMember(
                    dest => dest.Sku,
                    opt => opt.MapFrom(src => src.Sku != null ? src.Sku.Sku : null)
                )
                .ForMember(
                    dest => dest.ProductName,
                    opt =>
                        opt.MapFrom(src =>
                            src.Sku != null && src.Sku.Product != null
                                ? src.Sku.Product.ProductName
                                : null
                        )
                )
                .ForMember(
                    dest => dest.VariantAttributes,
                    opt => opt.MapFrom(src => src.Sku != null ? src.Sku.VariantAttributes : null)
                )
                .ForMember(
                    dest => dest.ImageUrl,
                    opt =>
                        opt.MapFrom(src =>
                            src.Sku != null
                                ? src
                                    .Sku.ProductImages.Where(pi => !pi.IsDeleted)
                                    .OrderByDescending(pi => pi.IsPrimary)
                                    .ThenBy(pi => pi.DisplayOrder)
                                    .Select(pi => pi.ImageUrl)
                                    .FirstOrDefault()
                                : null
                        )
                )
                .ForMember(
                    dest => dest.CurrentPrice,
                    opt => opt.MapFrom(src => src.Sku != null ? src.Sku.Price : 0)
                )
                .ForMember(
                    dest => dest.AvailableStock,
                    opt =>
                        opt.MapFrom(src =>
                            src.Sku != null && src.Sku.Inventory != null
                                ? src.Sku.Inventory.QuantityAvailable
                                : 0
                        )
                )
                .ForMember(
                    dest => dest.LineTotal,
                    opt => opt.MapFrom(src => src.PriceSnapshot * src.Quantity)
                );

            CreateMap<Category, CategoryDto>()
                .ForMember(
                    dest => dest.ParentCategoryName,
                    opt =>
                        opt.MapFrom(src =>
                            src.ParentCategory != null ? src.ParentCategory.CategoryName : null
                        )
                )
                .ForMember(
                    dest => dest.ProductCount,
                    opt =>
                        opt.MapFrom(src =>
                            src.ProductCategories.Count(pc =>
                                pc.Product != null && pc.Product.RemovedAt == null
                            )
                        )
                )
                .ForMember(
                    dest => dest.ChildCount,
                    opt => opt.MapFrom(src => src.ChildCategories.Count(cc => cc.IsActive))
                );

            // Order mappings
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                // Source navigation is `OrderItems`, target property is `Items` —
                // names differ so AutoMapper won't auto-resolve. Map explicitly.
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.OrderItems));

            CreateMap<OrderItem, OrderItemDto>()
                .ForMember(
                    dest => dest.Subtotal,
                    opt => opt.MapFrom(src => src.UnitPrice * src.Quantity)
                );

            CreateMap<Order, OrderSummaryDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(
                    dest => dest.Currency,
                    opt => opt.MapFrom(src => src.PreferredCurrency.ToString())
                )
                .ForMember(
                    dest => dest.TotalItems,
                    opt => opt.MapFrom(src => src.OrderItems.Sum(i => i.Quantity))
                );

            CreateMap<UserAddress, AddressDto>();

            CreateMap<Inventory, InventoryDto>();

            CreateMap<Coupon, CouponDto>();

            CreateMap<Inventory, InventoryDto>();

            // Review mappings
            CreateMap<Review, ReviewDto>()
                .ForMember(
                    dest => dest.Username,
                    opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty)
                )
                .ForMember(
                    dest => dest.AvatarUrl,
                    opt =>
                        opt.MapFrom(src =>
                            src.User != null && src.User.UserProfile != null
                                ? src.User.UserProfile.AvatarUrl
                                : null
                        )
                )
                .ForMember(
                    dest => dest.Sku,
                    opt => opt.MapFrom(src => src.OrderItem != null ? src.OrderItem.Sku : null)
                )
                .ForMember(
                    dest => dest.VariantDescription,
                    opt =>
                        opt.MapFrom(src =>
                            src.OrderItem != null ? src.OrderItem.VariantDescription : null
                        )
                )
                // Resolve the primary image of the purchased variant SKU.
                // Prefer IsPrimary, fall back to the first non-deleted image.
                .ForMember(
                    dest => dest.VariantImageUrl,
                    opt =>
                        opt.MapFrom(src =>
                            src.OrderItem != null && src.OrderItem.SkuNavigation != null
                                ? (
                                    src.OrderItem.SkuNavigation.ProductImages.Where(i =>
                                            !i.IsDeleted && i.IsPrimary
                                        )
                                        .Select(i => i.ImageUrl)
                                        .FirstOrDefault()
                                    ?? src.OrderItem.SkuNavigation.ProductImages.Where(i =>
                                            !i.IsDeleted
                                        )
                                        .Select(i => i.ImageUrl)
                                        .FirstOrDefault()
                                )
                                : null
                        )
                )
                .ForMember(
                    dest => dest.Images,
                    opt =>
                        opt.MapFrom(src => src.ReviewImages.OrderBy(ri => ri.DisplayOrder).ToList())
                );

            CreateMap<ReviewImage, ReviewImageDto>();
        }
    }
}
