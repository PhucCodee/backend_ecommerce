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

        public async Task<ProductDetailDto> GetByIdAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            return _mapper.Map<ProductDetailDto>(product);
        }

        public async Task<IEnumerable<ProductDetailDto>> GetAllAsync()
        {
            var products = await _productRepository.GetAllWithDetailsAsync();
            var activeProducts = products.Where(p => !p.IsDeleted());
            return _mapper.Map<IEnumerable<ProductDetailDto>>(activeProducts);
        }

        public async Task<PagedResult<ProductDetailDto>> GetAllPagedAsync(PaginationParams paginationParams, bool? primaryOnly = null)
        {
            var (products, totalCount) = await _productRepository.GetPagedAsync(
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var activeProducts = products.Where(p => !p.IsDeleted());
            
            // Filter for primary products only (products with is_default = true SKU)
            if (primaryOnly == true)
            {
                activeProducts = activeProducts.Where(p => p.ProductSkus.Any(s => s.IsDefault));
            }

            var productDtos = _mapper.Map<IEnumerable<ProductDetailDto>>(activeProducts);

            return PagedResult<ProductDetailDto>.Create(
                productDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount);
        }

        public async Task<IEnumerable<ProductSkuDetailDto>> GetVariantsAsync(int productId)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            // Return all non-default SKUs (variants)
            var variants = product.ProductSkus.Where(s => !s.IsDefault);
            return _mapper.Map<IEnumerable<ProductSkuDetailDto>>(variants);
        }

        public async Task<PagedResult<ProductDetailDto>> GetBySellerPagedAsync(int sellerId, PaginationParams paginationParams)
        {
            var (products, totalCount) = await _productRepository.GetBySellerPagedAsync(
                sellerId,
                paginationParams.PageNumber,
                paginationParams.PageSize);

            var activeProducts = products.Where(p => !p.IsDeleted());
            var productDtos = _mapper.Map<IEnumerable<ProductDetailDto>>(activeProducts);

            return PagedResult<ProductDetailDto>.Create(
                productDtos,
                paginationParams.PageNumber,
                paginationParams.PageSize,
                totalCount);
        }

        public async Task<ProductDetailDto> CreateAsync(ProductCreateDto createDto, int sellerId)
        {
            var slug = GenerateSlug(createDto.Name);
            var baseSku = $"SKU-{Guid.NewGuid():N}"[..12];

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

            var defaultSku = ProductSku.CreateDefault(
                product: product,
                sku: $"{baseSku}-DEFAULT",
                price: createDto.Price);

            var inventory = Inventory.CreateDefault(
                sku: defaultSku,
                quantityAvailable: createDto.Stock);

            defaultSku.Inventory = inventory;
            product.ProductSkus.Add(defaultSku);

            // Handle multiple images
            if (createDto.Images != null && createDto.Images.Count > 0)
            {
                for (int i = 0; i < createDto.Images.Count; i++)
                {
                    var imgDto = createDto.Images[i];
                    var image = ProductImage.CreateDefault(
                        product: product,
                        sku: defaultSku,
                        imageUrl: imgDto.ImageUrl,
                        altText: imgDto.AltText ?? createDto.Name,
                        isPrimary: imgDto.IsPrimary || i == 0); // First image is primary if none specified

                    image.DisplayOrder = imgDto.DisplayOrder > 0 ? imgDto.DisplayOrder : i + 1;
                    product.ProductImages.Add(image);
                    defaultSku.ProductImages.Add(image);
                }
            }
            // Fallback to single ImageUrl for backward compatibility
            else if (!string.IsNullOrWhiteSpace(createDto.ImageUrl))
            {
                var image = ProductImage.CreateDefault(
                    product: product,
                    sku: defaultSku,
                    imageUrl: createDto.ImageUrl,
                    altText: createDto.Name,
                    isPrimary: true);

                product.ProductImages.Add(image);
                defaultSku.ProductImages.Add(image);
            }

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDetailDto>(product);
        }

        public async Task<ProductDetailDto> UpdateAsync(int productId, ProductUpdateDto updateDto)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            ApplyUpdates(product, updateDto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDetailDto>(product);
        }

        public async Task<ProductDetailDto> UpdateSellerProductAsync(int productId, int sellerId, ProductUpdateDto updateDto)
        {
            var product = await _productRepository.GetByIdWithDetailsAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            if (product.SellerId != sellerId)
                throw new ForbiddenException("You do not have permission to update this product");

            ApplyUpdates(product, updateDto);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDetailDto>(product);
        }

        public async Task<bool> DeleteAsync(int productId)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            product.SoftDelete();
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteSellerProductAsync(int productId, int sellerId)
        {
            var product = await _productRepository.GetByIdAsync(productId)
                ?? throw new NotFoundException("Product not found");

            if (product.IsDeleted())
                throw new NotFoundException("Product not found");

            if (product.SellerId != sellerId)
                throw new ForbiddenException("You do not have permission to delete this product");

            product.SoftDelete();
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        private static void ApplyUpdates(Product product, ProductUpdateDto updateDto)
        {
            var now = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(updateDto.Name))
            {
                product.ProductName = updateDto.Name;
                product.Slug = GenerateSlug(updateDto.Name);
            }

            if (updateDto.Description != null)
                product.Description = updateDto.Description;

            if (updateDto.CategoryId.HasValue && updateDto.CategoryId.Value > 0)
                product.CategoryId = updateDto.CategoryId.Value;

            if (updateDto.Brand != null)
                product.Brand = updateDto.Brand;

            if (updateDto.WeightKg.HasValue)
                product.WeightKg = updateDto.WeightKg;

            if (updateDto.DimensionsCm != null)
                product.DimensionsCm = updateDto.DimensionsCm;

            if (!string.IsNullOrWhiteSpace(updateDto.Status) &&
                Enum.TryParse<ProductStatus>(updateDto.Status, true, out var status))
                product.Status = status;

            product.UpdatedAt = now;

            // Update SKU and inventory
            var defaultSku = product.ProductSkus.FirstOrDefault(ps => ps.IsDefault)
                ?? product.ProductSkus.FirstOrDefault();

            if (defaultSku != null)
            {
                if (updateDto.Price.HasValue)
                    defaultSku.Price = updateDto.Price.Value;

                defaultSku.UpdatedAt = now;

                if (updateDto.Stock.HasValue)
                {
                    if (defaultSku.Inventory != null)
                    {
                        defaultSku.Inventory.QuantityAvailable = updateDto.Stock.Value;
                        defaultSku.Inventory.UpdatedAt = now;
                    }
                    else
                    {
                        defaultSku.Inventory = Inventory.CreateDefault(
                            sku: defaultSku,
                            quantityAvailable: updateDto.Stock.Value);
                    }
                }

                // Handle multiple images update
                if (updateDto.Images != null && updateDto.Images.Count > 0)
                {
                    // Clear existing images and add new ones
                    product.ProductImages.Clear();
                    defaultSku.ProductImages.Clear();

                    for (int i = 0; i < updateDto.Images.Count; i++)
                    {
                        var imgDto = updateDto.Images[i];
                        var newImage = ProductImage.CreateDefault(
                            product: product,
                            sku: defaultSku,
                            imageUrl: imgDto.ImageUrl,
                            altText: imgDto.AltText ?? product.ProductName,
                            isPrimary: imgDto.IsPrimary || i == 0);

                        newImage.DisplayOrder = imgDto.DisplayOrder > 0 ? imgDto.DisplayOrder : i + 1;
                        product.ProductImages.Add(newImage);
                        defaultSku.ProductImages.Add(newImage);
                    }
                }
                // Fallback to single ImageUrl for backward compatibility
                else if (!string.IsNullOrWhiteSpace(updateDto.ImageUrl))
                {
                    var image = product.ProductImages.FirstOrDefault(pi => pi.IsPrimary)
                        ?? product.ProductImages.FirstOrDefault();

                    if (image != null)
                    {
                        image.ImageUrl = updateDto.ImageUrl;
                        image.ThumbnailUrl = updateDto.ImageUrl;
                        image.AltText = product.ProductName;
                        image.UpdatedAt = now;
                    }
                    else
                    {
                        var newImage = ProductImage.CreateDefault(
                            product: product,
                            sku: defaultSku,
                            imageUrl: updateDto.ImageUrl,
                            altText: product.ProductName,
                            isPrimary: true);

                        product.ProductImages.Add(newImage);
                        defaultSku.ProductImages.Add(newImage);
                    }
                }
            }
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