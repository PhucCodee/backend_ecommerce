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

        #region Read Operations

        public async Task<ProductSkuDetailDto> GetByIdAsync(int skuId)
        {
            var sku = await GetActiveSkuAsync(skuId);
            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        public async Task<IEnumerable<ProductSkuDetailDto>> GetByProductIdAsync(int productId)
        {
            await EnsureProductExistsAsync(productId);
            var skus = await _productSkuRepository.GetByProductIdWithDetailsAsync(productId);
            return _mapper.Map<IEnumerable<ProductSkuDetailDto>>(skus);
        }

        public async Task<PagedResult<ProductSkuDetailDto>> GetByProductIdPagedAsync(int productId, PaginationParams paginationParams)
        {
            await EnsureProductExistsAsync(productId);
            var (skus, totalCount) = await _productSkuRepository.GetByProductIdPagedAsync(
                productId, paginationParams.PageNumber, paginationParams.PageSize);
            return CreatePagedResult(skus, paginationParams, totalCount);
        }

        public async Task<PagedResult<ProductSkuDetailDto>> GetBySellerPagedAsync(int sellerId, PaginationParams paginationParams)
        {
            var (skus, totalCount) = await _productSkuRepository.GetBySellerPagedAsync(
                sellerId, paginationParams.PageNumber, paginationParams.PageSize);
            return CreatePagedResult(skus, paginationParams, totalCount);
        }

        #endregion

        #region Create Operations

        public async Task<ProductSkuDetailDto> CreateAsync(ProductSkuCreateDto createDto) =>
            await CreateInternalAsync(createDto, null);

        public async Task<ProductSkuDetailDto> CreateSellerSkuAsync(ProductSkuCreateDto createDto, int sellerId) =>
            await CreateInternalAsync(createDto, sellerId);

        private async Task<ProductSkuDetailDto> CreateInternalAsync(ProductSkuCreateDto createDto, int? sellerId)
        {
            var product = await GetActiveProductAsync(createDto.ProductId);

            if (sellerId.HasValue && product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to add SKU to this product");

            var sku = CreateSkuFromDto(product, createDto);

            await _productSkuRepository.AddAsync(sku);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        #endregion

        #region Update Operations

        public async Task<ProductSkuDetailDto> UpdateAsync(int skuId, ProductSkuUpdateDto updateDto) =>
            await UpdateInternalAsync(skuId, null, updateDto);

        public async Task<ProductSkuDetailDto> UpdateSellerSkuAsync(int skuId, int sellerId, ProductSkuUpdateDto updateDto) =>
            await UpdateInternalAsync(skuId, sellerId, updateDto);

        private async Task<ProductSkuDetailDto> UpdateInternalAsync(int skuId, int? sellerId, ProductSkuUpdateDto updateDto)
        {
            var sku = await GetActiveSkuAsync(skuId);

            if (sellerId.HasValue && sku.Product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to update this SKU");

            ApplyUpdates(sku, updateDto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductSkuDetailDto>(sku);
        }

        #endregion

        #region Delete Operations

        public async Task<bool> DeleteAsync(int skuId) =>
            await DeleteInternalAsync(skuId, null);

        public async Task<bool> DeleteSellerSkuAsync(int skuId, int sellerId) =>
            await DeleteInternalAsync(skuId, sellerId);

        private async Task<bool> DeleteInternalAsync(int skuId, int? sellerId)
        {
            var sku = await GetActiveSkuAsync(skuId);

            if (sellerId.HasValue && sku.Product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to delete this SKU");

            sku.IsActive = false;
            sku.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Private Helpers

        private async Task<ProductSku> GetActiveSkuAsync(int skuId)
        {
            var sku = await _productSkuRepository.GetByIdWithDetailsAsync(skuId)
                ?? throw new NotFoundException("Product SKU not found");

            if (!sku.IsActive || sku.Product.IsDeleted())
                throw new NotFoundException("Product SKU not found");

            return sku;
        }

        private async Task<Product> GetActiveProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            return product;
        }

        private async Task EnsureProductExistsAsync(int productId) => await GetActiveProductAsync(productId);

        private PagedResult<ProductSkuDetailDto> CreatePagedResult(
            IEnumerable<ProductSku> skus, PaginationParams paginationParams, int totalCount)
        {
            var dtos = _mapper.Map<IEnumerable<ProductSkuDetailDto>>(skus);
            return PagedResult<ProductSkuDetailDto>.Create(dtos, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
        }

        private static ProductSku CreateSkuFromDto(Product product, ProductSkuCreateDto dto)
        {
            var skuCode = $"{product.BaseSku}-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

            var sku = new ProductSku
            {
                Product = product,
                ProductId = product.ProductId,
                Sku = skuCode,
                VariantAttributes = dto.VariantAttributes,
                Price = dto.Price,
                CostPrice = dto.CostPrice,
                CompareAtPrice = dto.CompareAtPrice,
                IsActive = true,
                IsDefault = dto.IsDefault,
                WeightKg = dto.WeightKg,
                DimensionsCm = dto.DimensionsCm,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            sku.Inventory = Inventory.CreateDefault(sku, dto.Stock);

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var image = ProductImage.CreateDefault(product, sku, dto.ImageUrl, $"{product.ProductName} - {skuCode}", false);
                sku.ProductImages.Add(image);
            }

            return sku;
        }

        private static void ApplyUpdates(ProductSku sku, ProductSkuUpdateDto dto)
        {
            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.VariantAttributes)) sku.VariantAttributes = dto.VariantAttributes;
            if (dto.Price.HasValue) sku.Price = dto.Price.Value;
            if (dto.CostPrice.HasValue) sku.CostPrice = dto.CostPrice;
            if (dto.CompareAtPrice.HasValue) sku.CompareAtPrice = dto.CompareAtPrice;
            if (dto.IsActive.HasValue) sku.IsActive = dto.IsActive.Value;
            if (dto.IsDefault.HasValue) sku.IsDefault = dto.IsDefault.Value;
            if (dto.WeightKg.HasValue) sku.WeightKg = dto.WeightKg;
            if (dto.DimensionsCm != null) sku.DimensionsCm = dto.DimensionsCm;

            sku.UpdatedAt = now;

            // Update inventory
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

            // Update image
            if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
            {
                var image = sku.ProductImages.FirstOrDefault();
                if (image != null)
                {
                    image.ImageUrl = dto.ImageUrl;
                    image.ThumbnailUrl = dto.ImageUrl;
                    image.UpdatedAt = now;
                }
                else
                {
                    sku.ProductImages.Add(ProductImage.CreateDefault(sku.Product, sku, dto.ImageUrl, $"{sku.Product.ProductName} - {sku.Sku}", false));
                }
            }
        }

        #endregion
    }
}