using ECommerce.Application.DTOs.cart;
using System.Threading.Tasks;

namespace ECommerce.Application.Interfaces
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int? userId, string? sessionId);
        Task<CartDto> AddToCartAsync(int? userId, string? sessionId, AddToCartDto addDto, string? ipAddress = null);
        Task<CartDto> UpdateCartItemAsync(int? userId, string? sessionId, int cartItemId, UpdateCartItemDto updateDto);
        Task<CartDto> RemoveCartItemAsync(int? userId, string? sessionId, int cartItemId);
        Task<bool> ClearCartAsync(int? userId, string? sessionId);
        Task<CartDto> MergeCartsAsync(int userId, string sessionId);
    }
}