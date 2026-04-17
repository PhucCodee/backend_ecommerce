using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.cart;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.Exceptions;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController(ICartService cartService) : ControllerBase
    {
        private readonly ICartService _cartService = cartService;

        // Get current cart (supports both authenticated users and guests)
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var (userId, sessionId) = GetCartIdentifiers();
            var cart = await _cartService.GetCartAsync(userId, sessionId);
            return Ok(ApiResponse<CartDto>.Ok(cart));
        }

        // Add item to cart
        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDto addDto)
        {
            var (userId, sessionId) = GetCartIdentifiers();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var cart = await _cartService.AddToCartAsync(userId, sessionId, addDto, ipAddress);
            return Ok(ApiResponse<CartDto>.Ok(cart, "Item added to cart"));
        }

        // Update cart item quantity
        [HttpPut("items/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemDto updateDto)
        {
            var (userId, sessionId) = GetCartIdentifiers();
            var cart = await _cartService.UpdateCartItemAsync(userId, sessionId, cartItemId, updateDto);
            return Ok(ApiResponse<CartDto>.Ok(cart, "Cart item updated"));
        }

        // Remove item from cart
        [HttpDelete("items/{cartItemId}")]
        public async Task<IActionResult> RemoveCartItem(int cartItemId)
        {
            var (userId, sessionId) = GetCartIdentifiers();
            var cart = await _cartService.RemoveCartItemAsync(userId, sessionId, cartItemId);
            return Ok(ApiResponse<CartDto>.Ok(cart, "Item removed from cart"));
        }

        // Clear entire cart
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            var (userId, sessionId) = GetCartIdentifiers();
            await _cartService.ClearCartAsync(userId, sessionId);
            return Ok(ApiResponse<object>.Ok(new { }, "Cart cleared"));
        }

        // Merge guest cart with user cart after login
        [HttpPost("merge")]
        [Authorize]
        public async Task<IActionResult> MergeCarts()
        {
            var userId = GetCurrentUserId();
            var sessionId = GetSessionId();

            if (string.IsNullOrEmpty(sessionId))
                return Ok(ApiResponse<CartDto>.Ok(await _cartService.GetCartAsync(userId, null), "No guest cart to merge"));

            var cart = await _cartService.MergeCartsAsync(userId, sessionId);
            return Ok(ApiResponse<CartDto>.Ok(cart, "Carts merged successfully"));
        }

        private (int? userId, string? sessionId) GetCartIdentifiers()
        {
            int? userId = null;
            string? sessionId = null;

            // Try to get user ID if authenticated
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int parsedUserId))
            {
                userId = parsedUserId;
                // Authenticated users don't need sessionId - they have one lifetime cart
                return (userId, null);
            }

            // Get session ID from header for guest carts
            sessionId = GetSessionId();

            // If no session, generate one for guest
            if (string.IsNullOrEmpty(sessionId))
            {
                sessionId = System.Guid.NewGuid().ToString();
                Response.Headers.Append("X-Session-Id", sessionId);
            }

            return (userId, sessionId);
        }

        private string? GetSessionId()
        {
            return Request.Headers["X-Session-Id"].FirstOrDefault();
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