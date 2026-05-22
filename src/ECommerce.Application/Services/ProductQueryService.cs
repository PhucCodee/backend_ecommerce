using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.product;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services
{
    public class ProductQueryService(ApplicationDbContext context, IMapper mapper)
        : IProductQueryService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<ProductDetailDto> GetByIdAsync(int productId)
        {
            var product =
                await GetProductsForDetail().FirstOrDefaultAsync(p => p.ProductId == productId)
                ?? throw new NotFoundException("Product not found");

            return _mapper.Map<ProductDetailDto>(product);
        }

        public async Task<PagedResult<PublicProductSummaryDto>> GetPublicFilteredAsync(
            ProductQueryParams productQueryParams
        )
        {
            var dbQuery = productQueryParams.IncludeSuspended
                ? _context.Products.AsNoTracking()
                : _context.Products.AsNoTracking().Where(p => p.RemovedAt == null);

            dbQuery = ApplyFilters(dbQuery, productQueryParams);

            var totalCount = await dbQuery.CountAsync();

            dbQuery = ApplySorting(dbQuery, productQueryParams.SortBy, productQueryParams.Desc);

            var products = await dbQuery
                .AsSplitQuery()
                .Skip((productQueryParams.PageNumber - 1) * productQueryParams.PageSize)
                .Take(productQueryParams.PageSize)
                .Select(p => new PublicProductSummaryDto
                {
                    Id = p.ProductId,
                    Name = p.ProductName,
                    Slug = p.Slug,
                    Brand = p.Brand,
                    PrimaryCategoryName = p
                        .ProductCategories.Where(pc => pc.IsPrimary)
                        .Select(pc => pc.Category.CategoryName)
                        .FirstOrDefault(),
                    Price = p
                        .ProductSkus.Where(s => s.IsDefault)
                        .Select(s => s.Price)
                        .FirstOrDefault(),
                    CompareAtPrice = p
                        .ProductSkus.Where(s => s.IsDefault)
                        .Select(s => s.CompareAtPrice)
                        .FirstOrDefault(),
                    InStock = p.ProductSkus.Any(s =>
                        s.IsDefault && s.Inventory != null && s.Inventory.QuantityAvailable > 0
                    ),
                    ThumbnailUrl = p
                        .ProductSkus.Where(s => s.IsDefault)
                        .SelectMany(s => s.ProductImages)
                        .Where(i => !i.IsDeleted && i.IsPrimary)
                        .Select(i => i.ThumbnailUrl)
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return PagedResult<PublicProductSummaryDto>.Create(
                products,
                productQueryParams.PageNumber,
                productQueryParams.PageSize,
                totalCount
            );
        }

        public async Task<PagedResult<ProductSummaryDto>> GetFilteredAsync(
            ProductQueryParams productQueryParams
        )
        {
            var dbQuery = productQueryParams.IncludeSuspended
                ? _context.Products.AsNoTracking()
                : _context.Products.AsNoTracking().Where(p => p.RemovedAt == null);

            // Expand the requested categories into their descendant set when
            // the caller asks for it. This lets the buyer page show every
            // product under "Shoes" when "Shoes" is selected, not just the
            // ones tagged directly with that category.
            List<int>? categoryIdSet = null;
            if (productQueryParams.IncludeSubcategories && productQueryParams.CategoryIds.Count > 0)
            {
                var set = new HashSet<int>();
                foreach (var categoryId in productQueryParams.CategoryIds)
                {
                    foreach (var descendantId in await GetCategorySubtreeIdsAsync(categoryId))
                        set.Add(descendantId);
                }
                categoryIdSet = set.ToList();
            }

            dbQuery = ApplyFilters(dbQuery, productQueryParams, categoryIdSet);

            var totalCount = await dbQuery.CountAsync();

            dbQuery = ApplySorting(dbQuery, productQueryParams.SortBy, productQueryParams.Desc);

            var products = await dbQuery
                .AsSplitQuery()
                .Skip((productQueryParams.PageNumber - 1) * productQueryParams.PageSize)
                .Take(productQueryParams.PageSize)
                .Select(p => new ProductSummaryDto
                {
                    Id = p.ProductId,
                    Name = p.ProductName,
                    Slug = p.Slug,
                    Brand = p.Brand,
                    SellerId = p.SellerId,
                    SellerName = p.Seller != null ? p.Seller.Username : null,
                    PrimaryCategoryName = p
                        .ProductCategories.Where(pc => pc.IsPrimary)
                        .Select(pc => pc.Category.CategoryName)
                        .FirstOrDefault(),
                    CategoryNames = p
                        .ProductCategories.OrderByDescending(pc => pc.IsPrimary)
                        .Select(pc => pc.Category.CategoryName)
                        .ToList(),
                    CategoryIds = p
                        .ProductCategories.OrderByDescending(pc => pc.IsPrimary)
                        .Select(pc => pc.CategoryId)
                        .ToList(),
                    Price = p
                        .ProductSkus.Where(s => s.IsDefault)
                        .Select(s => s.Price)
                        .FirstOrDefault(),
                    CompareAtPrice = p
                        .ProductSkus.Where(s => s.IsDefault)
                        .Select(s => s.CompareAtPrice)
                        .FirstOrDefault(),
                    InStock = p.ProductSkus.Any(s =>
                        s.IsDefault && s.Inventory != null && s.Inventory.QuantityAvailable > 0
                    ),
                    ThumbnailUrl = p
                        .ProductSkus.Where(s => s.IsDefault)
                        .SelectMany(s => s.ProductImages)
                        .Where(i => !i.IsDeleted && i.IsPrimary)
                        .Select(i => i.ThumbnailUrl)
                        .FirstOrDefault(),
                    VariantCount = p.ProductSkus.Count(s => s.IsActive),
                    AverageRating = p
                        .ProductMetrics.OrderByDescending(m => m.Date)
                        .Select(m => m.AverageRating ?? 0)
                        .FirstOrDefault(),
                    ReviewCount = p
                        .ProductMetrics.OrderByDescending(m => m.Date)
                        .Select(m => m.ReviewCount)
                        .FirstOrDefault(),
                    Status = p.Status.ToString(),
                    IsSuspended = p.RemovedAt != null,
                })
                .ToListAsync();

            return PagedResult<ProductSummaryDto>.Create(
                products,
                productQueryParams.PageNumber,
                productQueryParams.PageSize,
                totalCount
            );
        }

        #region Private Helpers

        private IQueryable<Domain.Entities.Product> GetProductsForDetail() =>
            _context
                .Products.AsNoTracking()
                .AsSplitQuery()
                .Where(p => p.RemovedAt == null)
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.Seller)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.Inventory)
                .Include(p => p.ProductSkus)
                    .ThenInclude(sku => sku.ProductImages.Where(img => !img.IsDeleted))
                .Include(p => p.ProductMetrics);

        /// <summary>
        /// Walks the category tree starting at <paramref name="rootId"/> and
        /// returns the root plus every active descendant (BFS). Used to
        /// implement <c>IncludeSubcategories</c>.
        /// </summary>
        private async Task<List<int>> GetCategorySubtreeIdsAsync(int rootId)
        {
            var lookup = await _context
                .Categories.Where(c => c.IsActive)
                .Select(c => new { c.CategoryId, c.ParentCategoryId })
                .ToListAsync();

            var byParent = lookup
                .Where(c => c.ParentCategoryId.HasValue)
                .GroupBy(c => c.ParentCategoryId!.Value)
                .ToDictionary(g => g.Key, g => g.Select(c => c.CategoryId).ToList());

            var result = new HashSet<int> { rootId };
            var queue = new Queue<int>();
            queue.Enqueue(rootId);
            while (queue.Count > 0)
            {
                var id = queue.Dequeue();
                if (!byParent.TryGetValue(id, out var children))
                    continue;
                foreach (var childId in children)
                {
                    if (result.Add(childId))
                        queue.Enqueue(childId);
                }
            }
            return result.ToList();
        }

        private static IQueryable<Domain.Entities.Product> ApplyFilters(
            IQueryable<Domain.Entities.Product> query,
            ProductQueryParams p,
            List<int>? categoryIdSet = null
        )
        {
            if (p.MinPrice.HasValue)
                query = query.Where(x =>
                    x.ProductSkus.Any(s => s.IsDefault && s.Price >= p.MinPrice.Value)
                );

            if (p.MaxPrice.HasValue)
                query = query.Where(x =>
                    x.ProductSkus.Any(s => s.IsDefault && s.Price <= p.MaxPrice.Value)
                );

            // Category filtering:
            //  - `categoryIdSet` — the requested categories plus their
            //    descendants (set by GetFilteredAsync when IncludeSubcategories
            //    is on); a product matches if it is in ANY of them.
            //  - otherwise `CategoryIds` — a product must be tagged with ALL of
            //    the requested categories.
            if (categoryIdSet is { Count: > 0 })
            {
                query = query.Where(x =>
                    x.ProductCategories.Any(pc => categoryIdSet.Contains(pc.CategoryId))
                );
            }
            else
            {
                var categoryIds = p.CategoryIds?.Where(id => id > 0).Distinct().ToList() ?? [];
                if (categoryIds.Count > 0)
                {
                    query = query.Where(x =>
                        categoryIds.All(id => x.ProductCategories.Any(pc => pc.CategoryId == id))
                    );
                }
            }

            if (!string.IsNullOrWhiteSpace(p.Brand))
                query = query.Where(x =>
                    x.Brand != null && x.Brand.ToLower().Contains(p.Brand.ToLower())
                );

            if (p.SellerId.HasValue)
                query = query.Where(x => x.SellerId == p.SellerId.Value);

            if (
                !string.IsNullOrWhiteSpace(p.Status)
                && Enum.TryParse<ProductStatus>(p.Status, true, out var status)
            )
                query = query.Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(p.Search))
            {
                var searchLower = p.Search.ToLower();
                query = query.Where(x =>
                    x.ProductName.ToLower().Contains(searchLower)
                    || (x.Description != null && x.Description.ToLower().Contains(searchLower))
                    || (x.Brand != null && x.Brand.ToLower().Contains(searchLower))
                );
            }

            if (p.PrimaryOnly)
                query = query.Where(x => x.ProductSkus.Any(s => s.IsDefault));

            if (p.InStock.HasValue && p.InStock.Value)
                query = query.Where(x =>
                    x.ProductSkus.Any(s =>
                        s.IsDefault && s.Inventory != null && s.Inventory.QuantityAvailable > 0
                    )
                );

            return query;
        }

        private static IQueryable<Domain.Entities.Product> ApplySorting(
            IQueryable<Domain.Entities.Product> query,
            string? sortBy,
            bool desc
        )
        {
            return sortBy?.ToLower() switch
            {
                "price" => desc
                    ? query.OrderByDescending(p =>
                        p.ProductSkus.Where(s => s.IsDefault).Select(s => s.Price).FirstOrDefault()
                    )
                    : query.OrderBy(p =>
                        p.ProductSkus.Where(s => s.IsDefault).Select(s => s.Price).FirstOrDefault()
                    ),

                "name" or "a-z" => desc
                    ? query.OrderByDescending(p => p.ProductName)
                    : query.OrderBy(p => p.ProductName),

                "rating" => desc
                    ? query.OrderByDescending(p =>
                        p.ProductMetrics.Select(m => m.AverageRating).FirstOrDefault()
                    )
                    : query.OrderBy(p =>
                        p.ProductMetrics.Select(m => m.AverageRating).FirstOrDefault()
                    ),

                "popularity" or "views" => desc
                    ? query.OrderByDescending(p =>
                        p.ProductMetrics.Select(m => m.ViewCount).FirstOrDefault()
                    )
                    : query.OrderBy(p =>
                        p.ProductMetrics.Select(m => m.ViewCount).FirstOrDefault()
                    ),

                "sales" => desc
                    ? query.OrderByDescending(p =>
                        p.ProductMetrics.Select(m => m.PurchaseCount).FirstOrDefault()
                    )
                    : query.OrderBy(p =>
                        p.ProductMetrics.Select(m => m.PurchaseCount).FirstOrDefault()
                    ),

                "newest" or "createdat" => query.OrderByDescending(p => p.CreatedAt),
                "oldest" => query.OrderBy(p => p.CreatedAt),
                _ => query
                    .OrderBy(p =>
                        p.ProductCategories.Where(pc => pc.IsPrimary)
                            .Select(pc => pc.Category.DisplayOrder)
                            .FirstOrDefault()
                    )
                    .ThenBy(p => p.ProductName),
            };
        }

        #endregion
    }
}
