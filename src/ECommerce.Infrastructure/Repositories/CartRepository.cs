using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class CartRepository(ApplicationDbContext context)
        : Repository<Cart>(context),
            ICartRepository
    {
        private IQueryable<Cart> GetActiveCarts() =>
            _context.Carts.Where(c => c.Status == CartStatus.active);

        public async Task<Cart?> GetByUserIdWithDetailsAsync(int userId)
        {
            return await GetActiveCarts()
                .AsSplitQuery()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.Inventory)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetBySessionIdWithDetailsAsync(string sessionId)
        {
            return await GetActiveCarts()
                .AsSplitQuery()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.Inventory)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.ProductImages)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int skuId)
        {
            return await _context
                .Set<CartItem>()
                .Include(ci => ci.Sku)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && ci.SkuId == skuId);
        }

        public async Task AddCartItemAsync(CartItem cartItem)
        {
            await _context.Set<CartItem>().AddAsync(cartItem);
        }

        public async Task RemoveCartItemAsync(CartItem cartItem)
        {
            _context.Set<CartItem>().Remove(cartItem);
            await Task.CompletedTask;
        }
    }
}
