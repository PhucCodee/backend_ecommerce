using System.Collections.Generic;
using ECommerce.Application.Common.Pagination;

namespace ECommerce.Application.DTOs.product
{
    public class ProductQueryParams : PaginationParams
    {
        /// <summary>
        /// Sort by: "price", "name", "createdAt", "rating"
        /// </summary>
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort descending if true
        /// </summary>
        public bool Desc { get; set; } = false;

        /// <summary>
        /// Filter by minimum price
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Filter by maximum price
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Filter by category ID
        /// </summary>
        public List<int> CategoryIds { get; set; } = [];

        /// <summary>
        /// Filter by brand name
        /// </summary>
        public string? Brand { get; set; }

        /// <summary>
        /// Filter by seller ID
        /// </summary>
        public int? SellerId { get; set; }

        /// <summary>
        /// Filter by product status (e.g., "active", "draft")
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Search by keyword in name or description
        /// </summary>
        public string? Search { get; set; }

        /// <summary>
        /// Only return products with a default SKU
        /// </summary>
        public bool PrimaryOnly { get; set; } = true;

        /// <summary>
        /// Filter by in-stock products only
        /// </summary>
        public bool? InStock { get; set; }

        /// <summary>
        /// When true, includes soft-deleted (suspended) products in results.
        /// Should only be set for authenticated seller/admin queries.
        /// </summary>
        public bool IncludeSuspended { get; set; } = false;
    }
}
