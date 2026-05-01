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
using ECommerce.Application.DTOs.inventory;

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
                description: createDto.Description,
                brand: createDto.Brand,
                weightKg: createDto.WeightKg,
                dimensionsCm: createDto.DimensionsCm);

            foreach (var (catId, index) in createDto.CategoryIds.Select((id, i) => (id, i)))
            {
                product.ProductCategories.Add(new ProductCategory
                {
                    CategoryId = catId,
                    IsPrimary = index == 0  // first one is primary
                });
            }

            var defaultSku = ProductSku.CreateDefault(product, $"{baseSku}-DEFAULT", createDto.DefaultSkuPrice);
            defaultSku.Inventory = BuildInventory(defaultSku, createDto.DefaultSkuInventory, createDto.DefaultSkuStock);
            product.ProductSkus.Add(defaultSku);

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var created = await _productRepository.GetByIdWithDetailsAsync(product.ProductId)
                ?? throw new NotFoundException("Product not found");

            return _mapper.Map<ProductDto>(created);
        }

        public async Task<ProductDto> UpdateAsync(int productId, ProductUpdateDto updateDto, int? sellerId = null)
        {
            var product = await GetActiveProductAsync(productId);

            if (sellerId.HasValue && product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to update this product");

            var uniqueSlug = await ResolveSlugAsync(updateDto, productId);
            ApplyUpdates(product, updateDto, uniqueSlug);

            if (updateDto.CategoryIds != null)
                ReplaceProductCategories(product, updateDto.CategoryIds);

            await _unitOfWork.SaveChangesAsync();

            var loaded = await _productRepository.GetByIdWithDetailsAsync(productId)
                ?? throw new NotFoundException("Product not found");

            return _mapper.Map<ProductDto>(loaded);
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
        
        private static Inventory BuildInventory(ProductSku sku, InventoryCreateDto? dto, int fallbackStock)
        {
            var available = dto?.QuantityAvailable ?? fallbackStock;
            var reserved = dto?.QuantityReserved ?? 0;
            var sold = dto?.QuantitySold ?? 0;
        
            if (available < 0 || reserved < 0 || sold < 0)
                throw new BadRequestException("Inventory values must be non-negative");
        
            if (reserved > available)
                throw new BadRequestException("Reserved quantity cannot exceed available quantity");
        
            var inventory = Inventory.CreateDefault(sku, available);
            inventory.QuantityReserved = reserved;
            inventory.QuantitySold = sold;
            inventory.ReorderPoint = dto?.ReorderPoint ?? 0;
            inventory.ReorderQuantity = dto?.ReorderQuantity ?? 0;
            inventory.LastRestockedAt = dto?.LastRestockedAt ?? (available > 0 ? DateTime.UtcNow : null);
        
            return inventory;
        }

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
            if (updateDto.Brand != null) product.Brand = updateDto.Brand;
            if (updateDto.WeightKg.HasValue) product.WeightKg = updateDto.WeightKg;
            if (updateDto.DimensionsCm != null) product.DimensionsCm = updateDto.DimensionsCm;
            if (Enum.TryParse<ProductStatus>(updateDto.Status, true, out var status)) product.Status = status;

            product.UpdatedAt = DateTime.UtcNow;
        }

        private static void ReplaceProductCategories(Product product, List<int> categoryIds)
        {
            if (categoryIds.Count == 0)
                throw new BadRequestException("At least one category is required");

            var orderedDistinctIds = categoryIds.Distinct().ToList();
            var primaryId = orderedDistinctIds[0];
            var desired = orderedDistinctIds.ToHashSet();

            // remove missing (deletes join rows)
            var toRemove = product.ProductCategories.Where(pc => !desired.Contains(pc.CategoryId)).ToList();
            foreach (var pc in toRemove) product.ProductCategories.Remove(pc);

            // add new
            var existing = product.ProductCategories.Select(pc => pc.CategoryId).ToHashSet();
            foreach (var id in orderedDistinctIds)
                if (!existing.Contains(id))
                    product.ProductCategories.Add(new ProductCategory { CategoryId = id });

            // enforce primary
            foreach (var pc in product.ProductCategories)
                pc.IsPrimary = pc.CategoryId == primaryId;
        }

        #endregion
    }
}