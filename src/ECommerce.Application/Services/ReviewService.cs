using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.review;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Services
{
    public class ReviewService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper) : IReviewService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<PagedResult<ReviewDto>> GetByProductIdAsync(int productId, ReviewQueryParams query)
        {
            var dbQuery = _context.Reviews
                .Include(r => r.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(r => r.ReviewImages)
                .Where(r => r.ProductId == productId && r.IsApproved)
                .AsQueryable();

            if (query.Rating.HasValue)
                dbQuery = dbQuery.Where(r => r.Rating == query.Rating.Value);

            dbQuery = query.SortBy?.ToLowerInvariant() switch
            {
                "rating" => query.Desc ? dbQuery.OrderByDescending(r => r.Rating) : dbQuery.OrderBy(r => r.Rating),
                "helpful" => query.Desc ? dbQuery.OrderByDescending(r => r.HelpfulCount) : dbQuery.OrderBy(r => r.HelpfulCount),
                _ => query.Desc ? dbQuery.OrderByDescending(r => r.CreatedAt) : dbQuery.OrderBy(r => r.CreatedAt),
            };

            var totalCount = await dbQuery.CountAsync();
            var items = await dbQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var dtos = _mapper.Map<List<ReviewDto>>(items);
            return PagedResult<ReviewDto>.Create(dtos, query.PageNumber, query.PageSize, totalCount);
        }

        public async Task<ReviewSummaryDto> GetSummaryByProductIdAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .Select(r => r.Rating)
                .ToListAsync();

            var distribution = new Dictionary<int, int>
            {
                { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 }
            };
            foreach (var rating in reviews)
            {
                if (distribution.ContainsKey(rating))
                    distribution[rating]++;
            }

            return new ReviewSummaryDto
            {
                ProductId = productId,
                AverageRating = reviews.Count > 0 ? Math.Round((decimal)reviews.Average(), 1) : 0,
                ReviewCount = reviews.Count,
                RatingDistribution = distribution
            };
        }

        public async Task<List<ReviewableOrderItemDto>> GetReviewableItemsAsync(int productId, int userId)
        {
            var productSkuIds = await _context.ProductSkus
                .Where(s => s.ProductId == productId && s.IsActive)
                .Select(s => s.SkuId)
                .ToListAsync();

            if (productSkuIds.Count == 0) return [];

            var alreadyReviewedItemIds = await _context.Reviews
                .Where(r => r.ProductId == productId && r.UserId == userId)
                .Select(r => r.OrderItemId)
                .ToListAsync();

            var items = await _context.OrderItems
                .Include(oi => oi.Order)
                .Where(oi =>
                    productSkuIds.Contains(oi.SkuId) &&
                    oi.Order.UserId == userId &&
                    oi.Order.Status == Domain.Enums.OrderStatus.delivered &&
                    !alreadyReviewedItemIds.Contains(oi.OrderItemId))
                .OrderByDescending(oi => oi.Order.CreatedAt)
                .Select(oi => new ReviewableOrderItemDto
                {
                    OrderItemId = oi.OrderItemId,
                    OrderId = oi.OrderId,
                    OrderNumber = oi.Order.OrderNumber,
                    ProductName = oi.ProductName,
                    Sku = oi.Sku,
                    VariantDescription = oi.VariantDescription,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    OrderDate = oi.Order.CreatedAt
                })
                .ToListAsync();

            return items;
        }

        public async Task<ReviewDto> CreateAsync(ReviewCreateDto dto, int userId)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new BadRequestException("Rating must be between 1 and 5");

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.OrderItemId == dto.OrderItemId);

            if (orderItem == null)
                throw new NotFoundException("Order item not found");

            if (orderItem.Order.UserId != userId)
                throw new ForbiddenException("You can only review items from your own orders");

            if (orderItem.Order.Status != Domain.Enums.OrderStatus.delivered)
                throw new BadRequestException("You can only review items from delivered orders");

            var alreadyReviewed = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.OrderItemId == dto.OrderItemId);

            if (alreadyReviewed)
                throw new ConflictException("You have already reviewed this order item");

            var productExists = await _context.Products.AnyAsync(p => p.ProductId == dto.ProductId);
            if (!productExists)
                throw new NotFoundException("Product not found");

            var review = new Review
            {
                ProductId = dto.ProductId,
                UserId = userId,
                OrderItemId = dto.OrderItemId,
                Rating = dto.Rating,
                Title = dto.Title,
                Comment = dto.Comment,
                IsVerifiedPurchase = true,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Product = null!,
                OrderItem = null!,
                User = null!
            };

            _context.Reviews.Add(review);

            if (dto.ImageUrls is { Count: > 0 })
            {
                for (int i = 0; i < dto.ImageUrls.Count; i++)
                {
                    _context.ReviewImages.Add(new ReviewImage
                    {
                        Review = review,
                        ImageUrl = dto.ImageUrls[i],
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
            }

            await _context.SaveChangesAsync();
            await UpdateProductMetricsAsync(dto.ProductId);

            var created = await _context.Reviews
                .Include(r => r.User)
                    .ThenInclude(u => u.UserProfile)
                .Include(r => r.ReviewImages)
                .FirstAsync(r => r.ReviewId == review.ReviewId);

            return _mapper.Map<ReviewDto>(created);
        }

        public async Task<ReviewDto> UpdateAsync(int reviewId, ReviewUpdateDto dto, int userId)
        {
            var review = await _context.Reviews
                .Include(r => r.ReviewImages)
                .Include(r => r.User)
                    .ThenInclude(u => u.UserProfile)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId)
                ?? throw new NotFoundException("Review not found");

            if (review.UserId != userId)
                throw new ForbiddenException("You can only edit your own reviews");

            if (dto.Rating < 1 || dto.Rating > 5)
                throw new BadRequestException("Rating must be between 1 and 5");

            review.Rating = dto.Rating;
            if (dto.Title != null) review.Title = dto.Title;
            if (dto.Comment != null) review.Comment = dto.Comment;
            review.UpdatedAt = DateTime.UtcNow;

            if (dto.ImageUrls != null)
            {
                _context.ReviewImages.RemoveRange(review.ReviewImages);
                for (int i = 0; i < dto.ImageUrls.Count; i++)
                {
                    _context.ReviewImages.Add(new ReviewImage
                    {
                        Review = review,
                        ImageUrl = dto.ImageUrls[i],
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow,
                    });
                }
            }

            await _context.SaveChangesAsync();
            await UpdateProductMetricsAsync(review.ProductId);

            return _mapper.Map<ReviewDto>(review);
        }

        public async Task DeleteAsync(int reviewId, int userId)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId)
                ?? throw new NotFoundException("Review not found");

            if (review.UserId != userId)
                throw new ForbiddenException("You can only delete your own reviews");

            var productId = review.ProductId;
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            await UpdateProductMetricsAsync(productId);
        }

        private async Task UpdateProductMetricsAsync(int productId)
        {
            var stats = await _context.Reviews
                .Where(r => r.ProductId == productId && r.IsApproved)
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    Avg = g.Average(r => (decimal)r.Rating),
                    Count = g.Count()
                })
                .FirstOrDefaultAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var metric = await _context.ProductMetrics
                .FirstOrDefaultAsync(m => m.ProductId == productId && m.Date == today);

            if (metric != null)
            {
                metric.AverageRating = stats != null ? Math.Round(stats.Avg, 2) : null;
                metric.ReviewCount = stats?.Count ?? 0;
            }
            else
            {
                _context.ProductMetrics.Add(new ProductMetric
                {
                    ProductId = productId,
                    Date = today,
                    AverageRating = stats != null ? Math.Round(stats.Avg, 2) : null,
                    ReviewCount = stats?.Count ?? 0,
                    CreatedAt = DateTime.UtcNow,
                    Product = null!
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
