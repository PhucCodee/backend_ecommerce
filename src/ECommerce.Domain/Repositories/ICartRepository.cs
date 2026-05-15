using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface ICartRepository : IRepository<Cart>
    {
        Task<Cart?> GetByUserIdWithDetailsAsync(int userId);
        Task<Cart?> GetBySessionIdWithDetailsAsync(string sessionId);
        Task<CartItem?> GetCartItemAsync(int cartId, int skuId);
        Task AddCartItemAsync(CartItem cartItem);
        Task RemoveCartItemAsync(CartItem cartItem);
    }
}
