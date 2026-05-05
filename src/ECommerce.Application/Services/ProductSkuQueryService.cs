using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services
{
    public class ProductSkuQueryService(ApplicationDbContext context) : IProductSkuQueryService
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<ProductSkuDto> GetByIdAsync(int skuId)
        {
            var dto = await ActiveSkus()
                .Where(s => s.SkuId == skuId)
                .Select(s => new ProductSkuDto
                {
                    SkuId = s.SkuId,
                    ProductId = s.ProductId,
                    ProductName = s.Product.ProductName,
                    Sku = s.Sku,
                    VariantAttributes = s.VariantAttributes,
                    Price = s.Price,
                    CostPrice = s.CostPrice,
                    CompareAtPrice = s.CompareAtPrice,
                    IsActive = s.IsActive,
                    IsDefault = s.IsDefault,
                    WeightKg = s.WeightKg,
                    DimensionsCm = s.DimensionsCm,
                    Stock            = s.Inventory != null ? s.Inventory.QuantityAvailable  : 0,
                    QuantityReserved = s.Inventory != null ? s.Inventory.QuantityReserved   : 0,
                    QuantitySold     = s.Inventory != null ? s.Inventory.QuantitySold       : 0,
                    ReorderPoint     = s.Inventory != null ? s.Inventory.ReorderPoint       : 0,
                    ReorderQuantity  = s.Inventory != null ? s.Inventory.ReorderQuantity    : 0,
                    LastRestockedAt  = s.Inventory != null ? s.Inventory.LastRestockedAt    : null,
                    Images = s.ProductImages
                        .Where(i => !i.IsDeleted)
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new ProductImageDto
                        {
                            Id = i.ImageId,
                            ProductSkuId = i.SkuId,
                            ImageUrl = i.ImageUrl,
                            ThumbnailUrl = i.ThumbnailUrl,
                            AltText = i.AltText,
                            IsPrimary = i.IsPrimary,
                            DisplayOrder = i.DisplayOrder,
                            UpdatedAt = i.UpdatedAt
                        })
                        .ToList(),
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException("Product SKU not found");

            return dto;
        }

        public async Task<PagedResult<ProductSkuDto>> GetByProductIdPagedAsync(
            int productId, PaginationParams paginationParams)
        {
            var exists = await _context.Products.AnyAsync(p => p.ProductId == productId && p.RemovedAt == null);
            if (!exists) throw new NotFoundException("Product not found");

            var query = ActiveSkus().Where(s => s.ProductId == productId);
            return await ProjectPagedAsync(query, paginationParams);
        }

        public async Task<PagedResult<ProductSkuDto>> GetBySellerPagedAsync(
            int sellerId, PaginationParams paginationParams)
        {
            var query = ActiveSkus().Where(s => s.Product.SellerId == sellerId);
            return await ProjectPagedAsync(query, paginationParams);
        }

        #region Private Helpers

        private IQueryable<Domain.Entities.ProductSku> ActiveSkus() =>
            _context.ProductSkus
                .AsNoTracking()
                .Where(s => s.IsActive && s.Product.RemovedAt == null);

        private static async Task<PagedResult<ProductSkuDto>> ProjectPagedAsync(
            IQueryable<Domain.Entities.ProductSku> query, PaginationParams p)
        {
            var totalCount = await query.CountAsync();

            var dtos = await query
                .OrderByDescending(s => s.IsDefault)
                .ThenByDescending(s => s.CreatedAt)
                .Skip((p.PageNumber - 1) * p.PageSize)
                .Take(p.PageSize)
                .Select(s => new ProductSkuDto
                {
                    SkuId = s.SkuId,
                    ProductId = s.ProductId,
                    ProductName = s.Product.ProductName,
                    Sku = s.Sku,
                    VariantAttributes = s.VariantAttributes,
                    Price = s.Price,
                    CostPrice = s.CostPrice,
                    CompareAtPrice = s.CompareAtPrice,
                    IsActive = s.IsActive,
                    IsDefault = s.IsDefault,
                    WeightKg = s.WeightKg,
                    DimensionsCm = s.DimensionsCm,
                    Stock            = s.Inventory != null ? s.Inventory.QuantityAvailable  : 0,
                    QuantityReserved = s.Inventory != null ? s.Inventory.QuantityReserved   : 0,
                    QuantitySold     = s.Inventory != null ? s.Inventory.QuantitySold       : 0,
                    ReorderPoint     = s.Inventory != null ? s.Inventory.ReorderPoint       : 0,
                    ReorderQuantity  = s.Inventory != null ? s.Inventory.ReorderQuantity    : 0,
                    LastRestockedAt  = s.Inventory != null ? s.Inventory.LastRestockedAt    : null,
                    Images = s.ProductImages
                        .Where(i => !i.IsDeleted)
                        .OrderBy(i => i.DisplayOrder)
                        .Select(i => new ProductImageDto
                        {
                            Id = i.ImageId,
                            ProductSkuId = i.SkuId,
                            ImageUrl = i.ImageUrl,
                            ThumbnailUrl = i.ThumbnailUrl,
                            AltText = i.AltText,
                            IsPrimary = i.IsPrimary,
                            DisplayOrder = i.DisplayOrder,
                            UpdatedAt = i.UpdatedAt
                        })
                        .ToList(),
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return PagedResult<ProductSkuDto>.Create(dtos, p.PageNumber, p.PageSize, totalCount);
        }

        #endregion
    }
}