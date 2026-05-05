using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.review
{
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int OrderItemId { get; set; }
        /// <summary>
        /// SKU code of the variant the user actually purchased (from the
        /// OrderItem the review is linked to). Lets the FE distinguish reviews
        /// of different variants of the same product.
        /// </summary>
        public string? Sku { get; set; }
        /// <summary>
        /// Variant attribute description JSON snapshotted on the OrderItem at
        /// purchase time (e.g. <c>{"Color":"Black","Size":"M"}</c>).
        /// </summary>
        public string? VariantDescription { get; set; }
        /// <summary>
        /// Primary image URL of the variant SKU the reviewer purchased — used
        /// in place of the raw SKU code on review cards.
        /// </summary>
        public string? VariantImageUrl { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsVerifiedPurchase { get; set; }
        public int HelpfulCount { get; set; }
        public int UnhelpfulCount { get; set; }
        public List<ReviewImageDto> Images { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ReviewImageDto
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ReviewSummaryDto
    {
        public int ProductId { get; set; }
        public decimal AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public Dictionary<int, int> RatingDistribution { get; set; } = new();
    }

    public class ReviewCreateDto
    {
        public int ProductId { get; set; }
        public int OrderItemId { get; set; }
        public int Rating { get; set; }
        public required string Title { get; set; }
        public required string Comment { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class ReviewUpdateDto
    {
        public int Rating { get; set; }
        public string? Title { get; set; }
        public string? Comment { get; set; }
        public List<string>? ImageUrls { get; set; }
    }

    public class ReviewQueryParams
    {
        public int? Rating { get; set; }
        public string? SortBy { get; set; }
        public bool Desc { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class ReviewableOrderItemDto
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public required string OrderNumber { get; set; }
        public required string ProductName { get; set; }
        public required string Sku { get; set; }
        public string? VariantDescription { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
