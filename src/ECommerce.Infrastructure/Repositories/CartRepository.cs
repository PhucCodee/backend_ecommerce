using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class CartRepository(ApplicationDbContext context) : Repository<Cart>(context), ICartRepository
    {
        private IQueryable<Cart> GetActiveCarts() =>
            _context.Carts.Where(c => c.Status == CartStatus.active);

        public async Task<Cart?> GetByUserIdAsync(int userId)
        {
            return await GetActiveCarts()
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task<Cart?> GetBySessionIdAsync(string sessionId)
        {
            return await GetActiveCarts()
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);
        }

        public async Task<Cart?> GetByUserIdWithDetailsAsync(int userId)
        {
            return await GetActiveCarts()
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

        public async Task<Cart?> GetByIdWithDetailsAsync(int cartId)
        {
            return await GetActiveCarts()
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.Inventory)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Sku)
                        .ThenInclude(s => s.ProductImages)
                .FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        public async Task<CartItem?> GetCartItemByIdAsync(int cartItemId)
        {
            return await _context.Set<CartItem>()
                .Include(ci => ci.Sku)
                    .ThenInclude(s => s.Product)
                .Include(ci => ci.Sku)
                    .ThenInclude(s => s.Inventory)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);
        }

        public async Task<CartItem?> GetCartItemAsync(int cartId, int skuId)
        {
            return await _context.Set<CartItem>()
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