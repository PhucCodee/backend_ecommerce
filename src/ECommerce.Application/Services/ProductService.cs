using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Repositories;

namespace ECommerce.Application.Services
{
    public class ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var products = await _productRepository.GetAllWithDetailsAsync();
            return products.Select(MapToDto);
        }

        public async Task<ProductDto> GetProductByIdAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId) ?? throw new NotFoundException("Product not found");
            return MapToDto(product);
        }

        public async Task<ProductDto> CreateProductAsync(ProductDto productDto)
        {
            var now = DateTime.UtcNow;
            var slug = GenerateSlug(productDto.Name);
            var baseSku = $"SKU-{Guid.NewGuid():N}".Substring(0, 12);

            var product = new Product
            {
                ProductName = productDto.Name,
                Description = productDto.Description,
                SellerId = productDto.SellerId == 0 ? 1 : productDto.SellerId,
                CategoryId = productDto.CategoryId == 0 ? 1 : productDto.CategoryId,
                Slug = slug,
                BaseSku = baseSku,
                HasVariants = false,
                Brand = string.Empty,
                Status = ProductStatus.active,
                Moderation = ModerationStatus.approved,
                CreatedAt = now,
                UpdatedAt = now,
                Category = null!,
                Seller = null!,
            };

            var defaultSku = new ProductSku
            {
                Product = product,
                Sku = $"{baseSku}-DEFAULT",
                VariantAttributes = "{}",
                Price = productDto.Price,
                IsActive = true,
                IsDefault = true,
                CreatedAt = now,
                UpdatedAt = now,
            };

            var inventory = new Inventory
            {
                Sku = defaultSku,
                QuantityAvailable = productDto.Stock,
                QuantityReserved = 0,
                QuantitySold = 0,
                ReorderPoint = 0,
                ReorderQuantity = 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            defaultSku.Inventory = inventory;

            if (!string.IsNullOrWhiteSpace(productDto.ImageUrl))
            {
                var image = new ProductImage
                {
                    Product = product,
                    Sku = defaultSku,
                    ImageUrl = productDto.ImageUrl!,
                    ThumbnailUrl = productDto.ImageUrl,
                    AltText = productDto.Name,
                    DisplayOrder = 1,
                    IsPrimary = true,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                product.ProductImages.Add(image);
                defaultSku.ProductImages.Add(image);
            }

            product.ProductSkus.Add(defaultSku);

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(product);
        }

        public async Task<ProductDto> UpdateProductAsync(int productId, ProductDto productDto)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId) ?? throw new NotFoundException("Product not found");

            product.ProductName = productDto.Name;
            product.Description = productDto.Description;
            product.CategoryId = productDto.CategoryId == 0 ? product.CategoryId : productDto.CategoryId;
            product.SellerId = productDto.SellerId == 0 ? product.SellerId : productDto.SellerId;
            product.Slug = GenerateSlug(productDto.Name);

            var defaultSku = product.ProductSkus.FirstOrDefault(ps => ps.IsDefault) ?? product.ProductSkus.FirstOrDefault();
            if (defaultSku != null)
            {
                defaultSku.Price = productDto.Price;
                defaultSku.VariantAttributes ??= "{}";
                defaultSku.UpdatedAt = DateTime.UtcNow;

                if (defaultSku.Inventory != null)
                {
                    defaultSku.Inventory.QuantityAvailable = productDto.Stock;
                    defaultSku.Inventory.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    defaultSku.Inventory = new Inventory
                    {
                        Sku = defaultSku,
                        QuantityAvailable = productDto.Stock,
                        QuantityReserved = 0,
                        QuantitySold = 0,
                        ReorderPoint = 0,
                        ReorderQuantity = 0,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                }

                if (!string.IsNullOrWhiteSpace(productDto.ImageUrl))
                {
                    var image = product.ProductImages.FirstOrDefault() ?? new ProductImage
                    {
                        Product = product,
                        Sku = defaultSku,
                        ImageUrl = productDto.ImageUrl ?? string.Empty,
                        ThumbnailUrl = productDto.ImageUrl,
                        DisplayOrder = 1,
                        IsPrimary = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        AltText = productDto.Name
                    };

                    image.ImageUrl = productDto.ImageUrl!;
                    image.ThumbnailUrl = productDto.ImageUrl;
                    image.AltText = productDto.Name;
                    image.UpdatedAt = DateTime.UtcNow;

                    if (!product.ProductImages.Contains(image))
                    {
                        product.ProductImages.Add(image);
                        defaultSku.ProductImages.Add(image);
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return MapToDto(product);
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            product.SoftDelete();

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        private static ProductDto MapToDto(Product product)
        {
            var defaultSku = product.ProductSkus.FirstOrDefault(ps => ps.IsDefault) ?? product.ProductSkus.FirstOrDefault();
            var price = defaultSku?.Price ?? 0m;
            var stock = defaultSku?.Inventory?.QuantityAvailable ?? 0;
            var imageUrl = product.ProductImages.FirstOrDefault(pi => pi.IsPrimary)?.ImageUrl
                           ?? product.ProductImages.FirstOrDefault()?.ImageUrl;

            return new ProductDto
            {
                Id = product.ProductId,
                CategoryId = product.CategoryId,
                SellerId = product.SellerId,
                Name = product.ProductName,
                Description = product.Description,
                Price = price,
                Stock = stock,
                ImageUrl = imageUrl
            };
        }

        private static string GenerateSlug(string name)
        {
            var slug = new string([.. name.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')]);
            while (slug.Contains("--"))
            {
                slug = slug.Replace("--", "-");
            }

            return slug.Trim('-');
        }
    }
}