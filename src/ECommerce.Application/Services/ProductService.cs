using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Exceptions;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.DTOs.productsku;
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

        #region Read Operations

        public async Task<ProductDetailDto> GetByIdAsync(int productId)
        {
            var product = await GetActiveProductAsync(productId);
            return _mapper.Map<ProductDetailDto>(product);
        }

        public async Task<IEnumerable<ProductDetailDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<ProductDetailDto>>(products.Where(p => !p.IsDeleted()));
        }

        public async Task<PagedResult<ProductDetailDto>> GetAllPagedAsync(PaginationParams paginationParams, bool? primaryOnly = null)
        {
            var (products, totalCount) = await _productRepository.GetPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var activeProducts = products.Where(p => !p.IsDeleted());

            if (primaryOnly == true)
                activeProducts = activeProducts.Where(p => p.ProductSkus.Any(s => s.IsDefault));

            return CreatePagedResult(activeProducts, paginationParams, totalCount);
        }

        public async Task<PagedResult<ProductDetailDto>> GetFilteredAsync(ProductQueryParams query)
        {
            var (products, totalCount) = await _productRepository.GetFilteredPagedAsync(
                pageNumber: query.PageNumber,
                pageSize: query.PageSize,
                sortBy: query.SortBy,
                desc: query.Desc,
                minPrice: query.MinPrice,
                maxPrice: query.MaxPrice,
                categoryId: query.CategoryId,
                brand: query.Brand,
                sellerId: query.SellerId,
                status: query.Status,
                search: query.Search,
                primaryOnly: query.PrimaryOnly,
                inStock: query.InStock);

            var dtos = _mapper.Map<IEnumerable<ProductDetailDto>>(products);
            return PagedResult<ProductDetailDto>.Create(dtos, query.PageNumber, query.PageSize, totalCount);
        }

        public async Task<IEnumerable<ProductSkuDetailDto>> GetVariantsAsync(int productId)
        {
            var product = await GetActiveProductAsync(productId);
            var variants = product.ProductSkus.Where(s => !s.IsDefault);
            return _mapper.Map<IEnumerable<ProductSkuDetailDto>>(variants);
        }

        public async Task<PagedResult<ProductDetailDto>> GetBySellerPagedAsync(int sellerId, PaginationParams paginationParams)
        {
            var (products, totalCount) = await _productRepository.GetBySellerPagedAsync(
                sellerId,
                paginationParams.PageNumber,
                paginationParams.PageSize);

            return CreatePagedResult(products.Where(p => !p.IsDeleted()), paginationParams, totalCount);
        }

        #endregion

        #region Create Operations

        public async Task<ProductDetailDto> CreateAsync(ProductCreateDto createDto, int sellerId)
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

            var defaultSku = ProductSku.CreateDefault(product, $"{baseSku}-DEFAULT", createDto.Price);
            defaultSku.Inventory = Inventory.CreateDefault(defaultSku, createDto.Stock);
            product.ProductSkus.Add(defaultSku);

            AddImages(product, defaultSku, createDto.Images, createDto.ImageUrl, createDto.Name);

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDetailDto>(product);
        }

        #endregion

        #region Update Operations

        public async Task<ProductDetailDto> UpdateAsync(int productId, ProductUpdateDto updateDto) =>
            await UpdateInternalAsync(productId, null, updateDto);

        public async Task<ProductDetailDto> UpdateSellerProductAsync(int productId, int sellerId, ProductUpdateDto updateDto) =>
            await UpdateInternalAsync(productId, sellerId, updateDto);

        private async Task<ProductDetailDto> UpdateInternalAsync(int productId, int? sellerId, ProductUpdateDto updateDto)
        {
            var product = await GetActiveProductAsync(productId);

            if (sellerId.HasValue && product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to update this product");

            var uniqueSlug = await ResolveSlugAsync(updateDto, productId);
            ApplyUpdates(product, updateDto, uniqueSlug);

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ProductDetailDto>(product);
        }

        #endregion

        #region Delete Operations

        public async Task<bool> DeleteAsync(int productId) =>
            await DeleteInternalAsync(productId, null);

        public async Task<bool> DeleteSellerProductAsync(int productId, int sellerId) =>
            await DeleteInternalAsync(productId, sellerId);

        private async Task<bool> DeleteInternalAsync(int productId, int? sellerId)
        {
            var product = await GetActiveProductAsync(productId);

            if (sellerId.HasValue && product.SellerId != sellerId.Value)
                throw new ForbiddenException("You do not have permission to delete this product");

            product.SoftDelete();
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        #endregion

        #region Private Helpers

        private async Task<Product> GetActiveProductAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            return product;
        }

        private PagedResult<ProductDetailDto> CreatePagedResult(
            IEnumerable<Product> products, PaginationParams paginationParams, int totalCount)
        {
            var dtos = _mapper.Map<IEnumerable<ProductDetailDto>>(products);
            return PagedResult<ProductDetailDto>.Create(dtos, paginationParams.PageNumber, paginationParams.PageSize, totalCount);
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

        #endregion

        #region Apply Updates

        private static void ApplyUpdates(Product product, ProductUpdateDto updateDto, string? uniqueSlug)
        {
            var now = DateTime.UtcNow;

            // Product fields
            if (!string.IsNullOrWhiteSpace(updateDto.Name)) product.ProductName = updateDto.Name;
            if (uniqueSlug != null) product.Slug = uniqueSlug;
            if (updateDto.Description != null) product.Description = updateDto.Description;
            if (updateDto.CategoryId is > 0) product.CategoryId = updateDto.CategoryId.Value;
            if (updateDto.Brand != null) product.Brand = updateDto.Brand;
            if (updateDto.WeightKg.HasValue) product.WeightKg = updateDto.WeightKg;
            if (updateDto.DimensionsCm != null) product.DimensionsCm = updateDto.DimensionsCm;
            if (Enum.TryParse<ProductStatus>(updateDto.Status, true, out var status)) product.Status = status;

            product.UpdatedAt = now;

            // Update default SKU
            var defaultSku = product.ProductSkus.FirstOrDefault(ps => ps.IsDefault) ?? product.ProductSkus.FirstOrDefault();
            if (defaultSku == null) return;

            if (updateDto.Price.HasValue) defaultSku.Price = updateDto.Price.Value;
            defaultSku.UpdatedAt = now;

            UpdateInventory(defaultSku, updateDto.Stock, now);
            UpdateImages(product, defaultSku, updateDto, now);
        }

        private static void UpdateInventory(ProductSku sku, int? stock, DateTime now)
        {
            if (!stock.HasValue) return;

            if (sku.Inventory != null)
            {
                sku.Inventory.QuantityAvailable = stock.Value;
                sku.Inventory.UpdatedAt = now;
            }
            else
            {
                sku.Inventory = Inventory.CreateDefault(sku, stock.Value);
            }
        }

        #endregion

        #region Image Management

        private static void AddImages(Product product, ProductSku sku, List<ProductImageCreateDto>? images, string? imageUrl, string productName)
        {
            if (images?.Count > 0)
            {
                var primaryIndex = images.FindLastIndex(i => i.IsPrimary);
                if (primaryIndex < 0) primaryIndex = 0;

                for (int i = 0; i < images.Count; i++)
                {
                    var img = images[i];
                    var image = ProductImage.CreateDefault(product, sku, img.ImageUrl, img.AltText ?? productName, i == primaryIndex);
                    image.DisplayOrder = img.DisplayOrder > 0 ? img.DisplayOrder : i + 1;
                    product.ProductImages.Add(image);
                    sku.ProductImages.Add(image);
                }
            }
            else if (!string.IsNullOrWhiteSpace(imageUrl))
            {
                var image = ProductImage.CreateDefault(product, sku, imageUrl, productName, true);
                product.ProductImages.Add(image);
                sku.ProductImages.Add(image);
            }
        }

        private static void UpdateImages(Product product, ProductSku sku, ProductUpdateDto updateDto, DateTime now)
        {
            if (updateDto.Images?.Count > 0)
                UpsertImages(product, sku, updateDto.Images, now);
            else if (!string.IsNullOrWhiteSpace(updateDto.ImageUrl))
                UpdateOrAddPrimaryImage(product, sku, updateDto.ImageUrl, now);
        }

        private static void UpsertImages(Product product, ProductSku sku, List<ProductImageUpdateDto> images, DateTime now)
        {
            var existing = product.ProductImages.ToList();
            var toRemove = new List<ProductImage>();

            foreach (var dto in images)
            {
                if (dto.Id.HasValue)
                {
                    var img = existing.FirstOrDefault(e => e.ImageId == dto.Id.Value);
                    if (img == null) continue;

                    if (dto.IsDeleted)
                    {
                        toRemove.Add(img);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                        {
                            img.ImageUrl = dto.ImageUrl;
                            img.ThumbnailUrl = dto.ImageUrl;
                        }
                        img.AltText = dto.AltText ?? product.ProductName;
                        img.IsPrimary = dto.IsPrimary;
                        if (dto.DisplayOrder > 0) img.DisplayOrder = dto.DisplayOrder;
                        img.UpdatedAt = now;
                    }
                }
                else if (!dto.IsDeleted && !string.IsNullOrWhiteSpace(dto.ImageUrl))
                {
                    var newImg = ProductImage.CreateDefault(product, sku, dto.ImageUrl, dto.AltText ?? product.ProductName, dto.IsPrimary);
                    newImg.DisplayOrder = dto.DisplayOrder > 0 ? dto.DisplayOrder : product.ProductImages.Count + 1;
                    product.ProductImages.Add(newImg);
                    sku.ProductImages.Add(newImg);
                }
            }

            foreach (var img in toRemove)
            {
                product.ProductImages.Remove(img);
                sku.ProductImages.Remove(img);
            }

            EnsureSinglePrimaryImage(product);
        }

        private static void UpdateOrAddPrimaryImage(Product product, ProductSku sku, string imageUrl, DateTime now)
        {
            var primary = product.ProductImages.FirstOrDefault(pi => pi.IsPrimary) ?? product.ProductImages.FirstOrDefault();

            if (primary != null)
            {
                primary.ImageUrl = imageUrl;
                primary.ThumbnailUrl = imageUrl;
                primary.AltText = product.ProductName;
                primary.UpdatedAt = now;
            }
            else
            {
                var img = ProductImage.CreateDefault(product, sku, imageUrl, product.ProductName, true);
                product.ProductImages.Add(img);
                sku.ProductImages.Add(img);
            }
        }

        private static void EnsureSinglePrimaryImage(Product product)
        {
            var primaries = product.ProductImages.Where(i => i.IsPrimary).ToList();

            if (primaries.Count > 1)
                primaries.Take(primaries.Count - 1).ToList().ForEach(i => i.IsPrimary = false);
            else if (primaries.Count == 0 && product.ProductImages.Count > 0)
                product.ProductImages.First().IsPrimary = true;
        }

        #endregion
    }
}