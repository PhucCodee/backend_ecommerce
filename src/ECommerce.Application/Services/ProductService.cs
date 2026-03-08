using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Exceptions;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IProductService
    {
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<ProductDto> CreateAsync(ProductCreateDto createDto, int sellerId)
        {
            var slug = await GenerateUniqueSlugAsync(createDto.Name);
            var baseSku = GenerateBaseSku();

            var product = Product.CreateDefault(
                name: createDto.Name,
                slug: slug,
                baseSku: baseSku,
                sellerId: sellerId,
                categoryId: createDto.CategoryId == 0 ? 1 : createDto.CategoryId,
                description: createDto.Description,
                brand: createDto.Brand,
                weightKg: createDto.WeightKg,
                dimensionsCm: createDto.DimensionsCm);

            var defaultSku = ProductSku.CreateDefault(product, $"{baseSku}-DEFAULT", createDto.DefaultSkuPrice);
            defaultSku.Inventory = Inventory.CreateDefault(defaultSku, createDto.DefaultSkuStock);
            product.ProductSkus.Add(defaultSku);

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateAsync(int productId, ProductUpdateDto updateDto, int? sellerId = null)
        {
            var product = await GetActiveProductAsync(productId);

            if (sellerId.HasValue && product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to update this product");

            var uniqueSlug = await ResolveSlugAsync(updateDto, productId);
            ApplyUpdates(product, updateDto, uniqueSlug);

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteAsync(int productId, int? sellerId = null)
        {
            var product = await GetActiveProductAsync(productId);

            if (sellerId.HasValue && product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to delete this product");

            product.SoftDelete();
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        #region Private Helpers

        private async Task<Product> GetActiveProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            return product;
        }

        private async Task<string?> ResolveSlugAsync(ProductUpdateDto updateDto, int productId)
        {
            var slugSource = updateDto.Slug ?? updateDto.Name;
            return string.IsNullOrWhiteSpace(slugSource) ? null : await GenerateUniqueSlugAsync(slugSource, productId);
        }

        private static string GenerateBaseSku() => $"SKU-{Guid.NewGuid():N}"[..12];

        private static string GenerateSlug(string name)
        {
            var slug = string.Concat(name.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '-'));
            while (slug.Contains("--")) slug = slug.Replace("--", "-");
            return slug.Trim('-');
        }

        private async Task<string> GenerateUniqueSlugAsync(string name, int? excludeProductId = null)
        {
            var baseSlug = GenerateSlug(name);
            var slug = baseSlug;
            var counter = 1;

            while (await _productRepository.SlugExistsAsync(slug, excludeProductId))
                slug = $"{baseSlug}-{counter++}";

            return slug;
        }

        private static void ApplyUpdates(Product product, ProductUpdateDto updateDto, string? uniqueSlug)
        {
            if (!string.IsNullOrWhiteSpace(updateDto.Name)) product.ProductName = updateDto.Name;
            if (uniqueSlug != null) product.Slug = uniqueSlug;
            if (updateDto.Description != null) product.Description = updateDto.Description;
            if (updateDto.CategoryId is > 0) product.CategoryId = updateDto.CategoryId.Value;
            if (updateDto.Brand != null) product.Brand = updateDto.Brand;
            if (updateDto.WeightKg.HasValue) product.WeightKg = updateDto.WeightKg;
            if (updateDto.DimensionsCm != null) product.DimensionsCm = updateDto.DimensionsCm;
            if (Enum.TryParse<ProductStatus>(updateDto.Status, true, out var status)) product.Status = status;

            product.UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}