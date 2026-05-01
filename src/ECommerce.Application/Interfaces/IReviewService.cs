using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.DTOs.review;

namespace ECommerce.Application.Interfaces
{
    public interface IReviewService
    {
        Task<PagedResult<ReviewDto>> GetByProductIdAsync(int productId, ReviewQueryParams query);
        Task<ReviewSummaryDto> GetSummaryByProductIdAsync(int productId);
        Task<List<ReviewableOrderItemDto>> GetReviewableItemsAsync(int productId, int userId);
        Task<ReviewDto> CreateAsync(ReviewCreateDto dto, int userId);
        Task<ReviewDto> UpdateAsync(int reviewId, ReviewUpdateDto dto, int userId);
        Task DeleteAsync(int reviewId, int userId);
    }
}
