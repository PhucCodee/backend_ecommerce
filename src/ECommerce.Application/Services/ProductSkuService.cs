using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.DTOs.inventory;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Helpers;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class ProductSkuService(
        IProductSkuRepository productSkuRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper
    ) : IProductSkuService
    {
        private readonly IProductSkuRepository _productSkuRepository = productSkuRepository;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        private static readonly HashSet<string> AllowedColors = Enum.GetNames(typeof(ProductColor))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        private static readonly HashSet<string> AllowedSizes = Enum.GetNames(typeof(ProductSize))
            .Select(n => n.StartsWith("EU") ? n[2..] : n)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        public async Task<ProductSkuDto> CreateAsync(
            ProductSkuCreateDto createDto,
            int? sellerId = null
        )
        {
            var product = await GetActiveProductAsync(createDto.ProductId);

            if (sellerId.HasValue && product.SellerId != sellerId.Value)
                throw new ForbiddenException(
                    "You do not have permission to add SKU to this product"
                );

            ValidateColorAndSize(createDto.Color, createDto.Size);

            var sku = CreateSkuFromDto(product, createDto);

            await _productSkuRepository.AddAsync(sku);
            await _unitOfWork.SaveChangesAsync();

            sku.Sku = SkuHelper.GenerateVariantSku(product.BaseSku, sku.SkuId);
            ImageHelper.AddImages(sku, createDto.Images, $"{product.ProductName} - {sku.Sku}");
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDto>(sku);
        }

        public async Task<ProductSkuDto> UpdateAsync(
            int skuId,
            ProductSkuUpdateDto updateDto,
            int? sellerId = null
        )
        {
            var sku = await GetActiveSkuAsync(skuId);

            if (sellerId.HasValue && sku.Product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to update this SKU");

            ValidateColorAndSize(updateDto.Color, updateDto.Size);

            ApplyUpdates(sku, updateDto);

            if (updateDto.IsDefault == true)
            {
                var siblings = await _productSkuRepository.GetByProductIdAsync(sku.ProductId);
                foreach (var sibling in siblings.Where(s => s.SkuId != skuId && s.IsDefault))
                {
                    sibling.IsDefault = false;
                    sibling.UpdatedAt = DateTime.UtcNow;
                }
            }
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDto>(sku);
        }

        public async Task<bool> DeleteAsync(int skuId, int? sellerId = null)
        {
            var sku = await GetActiveSkuAsync(skuId);

            if (sellerId.HasValue && sku.Product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to delete this SKU");

            if (await _productSkuRepository.HasActiveOrdersAsync(skuId))
                throw new BadRequestException(
                    "This SKU is part of an active order and cannot be deleted. "
                        + "Wait until those orders are delivered or cancelled."
                );

            sku.IsActive = false;
            sku.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ProductSkuDto> RestoreAsync(int skuId, int? sellerId = null)
        {
            var sku =
                await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (sku.Product.IsDeleted())
                throw new BadRequestException(
                    "Cannot restore a SKU whose parent product is deleted"
                );

            if (sellerId.HasValue && sku.Product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to restore this SKU");

            if (sku.IsActive)
                return _mapper.Map<ProductSkuDto>(sku);

            sku.IsActive = true;
            sku.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDto>(sku);
        }

        private async Task<ProductSku> GetActiveSkuAsync(int skuId)
        {
            var sku =
                await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive || sku.Product.IsDeleted())
                throw new NotFoundException("Product SKU not found");

            return sku;
        }

        private async Task<Product> GetActiveProductAsync(int productId)
        {
            var product =
                await _productRepository.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            return product;
        }

        private static ProductSku CreateSkuFromDto(Product product, ProductSkuCreateDto dto)
        {
            var sku = new ProductSku
            {
                Product = product,
                ProductId = product.ProductId,
                Sku = "",
                Color = NormalizeColor(dto.Color),
                Size = NormalizeSize(dto.Size),
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                CompareAtPrice = dto.CompareAtPrice,
                IsActive = true,
                IsDefault = false,
                WeightKg = dto.WeightKg,
                DimensionsCm = dto.DimensionsCm,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            sku.Inventory = BuildInventory(sku, dto.Inventory, dto.Stock);

            return sku;
        }

        private static Inventory BuildInventory(
            ProductSku sku,
            InventoryCreateDto? dto,
            int fallbackStock
        )
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
            inventory.LastRestockedAt =
                dto?.LastRestockedAt ?? (available > 0 ? DateTime.UtcNow : null);

            return inventory;
        }

        private static void ApplyUpdates(ProductSku sku, ProductSkuUpdateDto dto)
        {
            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Color))
                sku.Color = NormalizeColor(dto.Color);
            if (!string.IsNullOrWhiteSpace(dto.Size))
                sku.Size = NormalizeSize(dto.Size);
            if (dto.Price.HasValue)
                sku.Price = dto.Price.Value;
            if (dto.CostPrice.HasValue)
                sku.CostPrice = dto.CostPrice;
            if (dto.CompareAtPrice.HasValue)
                sku.CompareAtPrice = dto.CompareAtPrice;
            if (dto.IsActive.HasValue)
                sku.IsActive = dto.IsActive.Value;
            if (dto.IsDefault.HasValue)
                sku.IsDefault = dto.IsDefault.Value;
            if (dto.WeightKg.HasValue)
                sku.WeightKg = dto.WeightKg;
            if (dto.DimensionsCm != null)
                sku.DimensionsCm = dto.DimensionsCm;

            sku.UpdatedAt = now;

            if (dto.Stock.HasValue)
            {
                if (sku.Inventory != null)
                {
                    sku.Inventory.QuantityAvailable = dto.Stock.Value;
                    sku.Inventory.UpdatedAt = now;
                }
                else
                {
                    sku.Inventory = Inventory.CreateDefault(sku, dto.Stock.Value);
                }
            }

            if (dto.Images?.Count > 0)
                ImageHelper.MergeImages(
                    sku,
                    dto.Images,
                    now,
                    $"{sku.Product.ProductName} - {sku.Sku}"
                );
        }

        private static void ValidateColorAndSize(string? color, string? size)
        {
            if (!string.IsNullOrWhiteSpace(color) && !AllowedColors.Contains(color))
                throw new BadRequestException("Invalid color");

            if (!string.IsNullOrWhiteSpace(size) && !AllowedSizes.Contains(size))
                throw new BadRequestException("Invalid size");
        }

        private static string? NormalizeColor(string? color)
        {
            if (string.IsNullOrWhiteSpace(color))
                return null;

            if (!Enum.TryParse<ProductColor>(color, true, out var parsed))
                throw new BadRequestException("Invalid color");

            return parsed.ToString();
        }

        private static string? NormalizeSize(string? size)
        {
            if (string.IsNullOrWhiteSpace(size))
                return null;

            if (Enum.TryParse<ProductSize>(size, true, out var parsed))
            {
                var name = parsed.ToString();
                return name.StartsWith("EU", StringComparison.Ordinal) ? name[2..] : name;
            }

            throw new BadRequestException("Invalid size");
        }
    }
}
