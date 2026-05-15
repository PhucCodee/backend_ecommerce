using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Application.Common.Authorization;
using ECommerce.Application.Common.Pagination;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.DTOs.review;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController(IReviewService reviewService) : ControllerBase
    {
        private readonly IReviewService _reviewService = reviewService;

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProductId(
            int productId,
            [FromQuery] ReviewQueryParams query
        )
        {
            var reviews = await _reviewService.GetByProductIdAsync(productId, query);
            return Ok(ApiResponse<PagedResult<ReviewDto>>.Ok(reviews));
        }

        [HttpGet("product/{productId:int}/summary")]
        public async Task<IActionResult> GetSummary(int productId)
        {
            var summary = await _reviewService.GetSummaryByProductIdAsync(productId);
            return Ok(ApiResponse<ReviewSummaryDto>.Ok(summary));
        }

        [HttpGet("product/{productId:int}/reviewable-items")]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<IActionResult> GetReviewableItems(int productId)
        {
            var userId = GetCurrentUserId();
            var items = await _reviewService.GetReviewableItemsAsync(productId, userId);
            return Ok(
                ApiResponse<System.Collections.Generic.List<ReviewableOrderItemDto>>.Ok(items)
            );
        }

        [HttpPost]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<IActionResult> Create([FromBody] ReviewCreateDto dto)
        {
            var userId = GetCurrentUserId();
            var review = await _reviewService.CreateAsync(dto, userId);
            return StatusCode(
                201,
                ApiResponse<ReviewDto>.Ok(review, "Review created successfully")
            );
        }

        [HttpPut("{reviewId:int}")]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<IActionResult> Update(int reviewId, [FromBody] ReviewUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            var review = await _reviewService.UpdateAsync(reviewId, dto, userId);
            return Ok(ApiResponse<ReviewDto>.Ok(review, "Review updated successfully"));
        }

        [HttpDelete("{reviewId:int}")]
        [Authorize(Policy = Policies.Authenticated)]
        public async Task<IActionResult> Delete(int reviewId)
        {
            var userId = GetCurrentUserId();
            await _reviewService.DeleteAsync(reviewId, userId);
            return Ok(ApiResponse<object>.Ok(new { reviewId }, "Review deleted successfully"));
        }

        // Mark review as helpful — open to anyone (incl. anonymous browsers)
        // since the FE deduplicates per-browser via localStorage.
        [HttpPost("{reviewId:int}/helpful")]
        public async Task<IActionResult> MarkHelpful(int reviewId)
        {
            var review = await _reviewService.MarkHelpfulAsync(reviewId);
            return Ok(ApiResponse<ReviewDto>.Ok(review, "Marked as helpful"));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedException("Invalid user token");
            return userId;
        }
    }
}
