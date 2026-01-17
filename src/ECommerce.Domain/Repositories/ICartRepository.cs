using ECommerce.Domain.Entities;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetByUserIdAsync(int userId);
        Task<Cart?> GetBySessionIdAsync(string sessionId);
        Task<Cart?> GetByUserIdWithDetailsAsync(int userId);
        Task<Cart?> GetBySessionIdWithDetailsAsync(string sessionId);
        Task<Cart?> GetByIdWithDetailsAsync(int cartId);
        Task<CartItem?> GetCartItemByIdAsync(int cartItemId);
        Task<CartItem?> GetCartItemAsync(int cartId, int skuId);
        Task AddCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(CartItem cartItem);
    }
}