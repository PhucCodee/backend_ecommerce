using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.productsku;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;

namespace ECommerce.Application.Services
{
    public class ProductSkuService(
        IProductSkuRepository productSkuRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IProductSkuService
    {
        private readonly IProductSkuRepository _productSkuRepository = productSkuRepository;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<ProductSkuDetailDto> GetByIdAsync(int skuId)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive || sku.Product.IsDeleted())
                throw new NotFoundException("Product SKU not found");

            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        public async Task<IEnumerable<ProductSkuDetailDto>> GetByProductIdAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            var skus = await _productSkuRepository.GetByProductIdWithDetailsAsync(productId);
            return _mapper.Map<IEnumerable<ProductSkuDetailDto>>(skus);
        }

        public async Task<PagedResult<ProductSkuDetailDto>> GetByProductIdPagedAsync(
            int productId, PaginationParams paginationParams)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            var (skus, totalCount) = await _productSkuRepository.GetByProductIdPagedAsync(
                productId,
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var skuDtos = _mapper.Map<IEnumerable<ProductSkuDetailDto>>(skus);

            return PagedResult<ProductSkuDetailDto>.Create(
                skuDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount);
        }

        public async Task<PagedResult<ProductSkuDetailDto>> GetBySellerPagedAsync(
            int sellerId, PaginationParams paginationParams)
        {
            var (skus, totalCount) = await _productSkuRepository.GetBySellerPagedAsync(
                sellerId,
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var skuDtos = _mapper.Map<IEnumerable<ProductSkuDetailDto>>(skus);

            return PagedResult<ProductSkuDetailDto>.Create(
                skuDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount);
        }

        public async Task<ProductSkuDetailDto> CreateAsync(ProductSkuCreateDto createDto)
        {
            var product = await _productRepository.GetByIdAsync(createDto.ProductId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            var sku = CreateSkuFromDto(product, createDto);

            await _productSkuRepository.AddAsync(sku);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        public async Task<ProductSkuDetailDto> CreateSellerSkuAsync(ProductSkuCreateDto createDto, int sellerId)
        {
            var product = await _productRepository.GetByIdAsync(createDto.ProductId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            if (product.SellerId != sellerId)
                throw new ForbiddenException("You do not have permission to add SKU to this product");

            var sku = CreateSkuFromDto(product, createDto);

            await _productSkuRepository.AddAsync(sku);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        public async Task<ProductSkuDetailDto> UpdateAsync(int skuId, ProductSkuUpdateDto updateDto)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive || sku.Product.IsDeleted())
                throw new NotFoundException("Product SKU not found");

            ApplyUpdates(sku, updateDto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        public async Task<ProductSkuDetailDto> UpdateSellerSkuAsync(
            int skuId, int sellerId, ProductSkuUpdateDto updateDto)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive || sku.Product.IsDeleted())
                throw new NotFoundException("Product SKU not found");

            if (sku.Product.SellerId != sellerId)
                throw new ForbiddenException("You do not have permission to update this SKU");

            ApplyUpdates(sku, updateDto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        public async Task<bool> DeleteAsync(int skuId)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive)
                throw new NotFoundException("Product SKU not found");

            // Soft delete by deactivating
            sku.IsActive = false;
            sku.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSellerSkuAsync(int skuId, int sellerId)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive)
                throw new NotFoundException("Product SKU not found");

            if (sku.Product.SellerId != sellerId)
                throw new ForbiddenException("You do not have permission to delete this SKU");

            // Soft delete by deactivating
            sku.IsActive = false;
            sku.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static ProductSku CreateSkuFromDto(Product product, ProductSkuCreateDto createDto)
        {
            var now = DateTime.UtcNow;
            var skuCode = $"{product.BaseSku}-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

            var sku = new ProductSku
            {
                Product = product,
                ProductId = product.ProductId,
                Sku = skuCode,
                VariantAttributes = createDto.VariantAttributes,
                Price = createDto.Price,
                CostPrice = createDto.CostPrice,
                CompareAtPrice = createDto.CompareAtPrice,
                IsActive = true,
                IsDefault = createDto.IsDefault,
                WeightKg = createDto.WeightKg,
                DimensionsCm = createDto.DimensionsCm,
                CreatedAt = now,
                UpdatedAt = now
            };

            var inventory = Inventory.CreateDefault(
                sku: sku,
                quantityAvailable: createDto.Stock);

            sku.Inventory = inventory;

            if (!string.IsNullOrWhiteSpace(createDto.ImageUrl))
            {
                var image = ProductImage.CreateDefault(
                    product: product,
                    sku: sku,
                    imageUrl: createDto.ImageUrl,
                    altText: $"{product.ProductName} - {skuCode}",
                    isPrimary: false);

                sku.ProductImages.Add(image);
            }

            return sku;
        }

        private static void ApplyUpdates(ProductSku sku, ProductSkuUpdateDto updateDto)
        {
            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(updateDto.VariantAttributes))
                sku.VariantAttributes = updateDto.VariantAttributes;

            if (updateDto.Price.HasValue)
                sku.Price = updateDto.Price.Value;

            if (updateDto.CostPrice.HasValue)
                sku.CostPrice = updateDto.CostPrice;

            if (updateDto.CompareAtPrice.HasValue)
                sku.CompareAtPrice = updateDto.CompareAtPrice;

            if (updateDto.IsActive.HasValue)
                sku.IsActive = updateDto.IsActive.Value;

            if (updateDto.IsDefault.HasValue)
                sku.IsDefault = updateDto.IsDefault.Value;

            if (updateDto.WeightKg.HasValue)
                sku.WeightKg = updateDto.WeightKg;

            if (updateDto.DimensionsCm != null)
                sku.DimensionsCm = updateDto.DimensionsCm;

            sku.UpdatedAt = now;

            // Update inventory
            if (updateDto.Stock.HasValue)
            {
                if (sku.Inventory != null)
                {
                    sku.Inventory.QuantityAvailable = updateDto.Stock.Value;
                    sku.Inventory.UpdatedAt = now;
                }
                else
                {
                    sku.Inventory = Inventory.CreateDefault(
                        sku: sku,
                        quantityAvailable: updateDto.Stock.Value);
                }
            }

            // Update image
            if (!string.IsNullOrWhiteSpace(updateDto.ImageUrl))
            {
                var image = sku.ProductImages.FirstOrDefault();

                if (image != null)
                {
                    image.ImageUrl = updateDto.ImageUrl;
                    image.ThumbnailUrl = updateDto.ImageUrl;
                    image.UpdatedAt = now;
                }
                else
                {
                    var newImage = ProductImage.CreateDefault(
                        product: sku.Product,
                        sku: sku,
                        imageUrl: updateDto.ImageUrl,
                        altText: $"{sku.Product.ProductName} - {sku.Sku}",
                        isPrimary: false);

                    sku.ProductImages.Add(newImage);
                }
            }
        }
    }
}