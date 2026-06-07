using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class InventoryRepository(ApplicationDbContext context)
        : Repository<Inventory>(context),
            IInventoryRepository
    {
        public async Task<bool> TryReserveStockAsync(int skuId, int quantity)
        {
            var affectedRows = await _context
                .Inventories.Where(i => i.SkuId == skuId && i.QuantityAvailable >= quantity)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(i => i.QuantityAvailable, i => i.QuantityAvailable - quantity)
                        .SetProperty(i => i.QuantityReserved, i => i.QuantityReserved + quantity)
                );

            return affectedRows == 1;
        }

        public async Task<bool> TryReleaseReservedStockAsync(int skuId, int quantity)
        {
            var affectedRows = await _context
                .Inventories.Where(i => i.SkuId == skuId && i.QuantityReserved >= quantity)
                .ExecuteUpdateAsync(s =>
                    s.SetProperty(i => i.QuantityAvailable, i => i.QuantityAvailable + quantity)
                        .SetProperty(i => i.QuantityReserved, i => i.QuantityReserved - quantity)
                );

            return affectedRows == 1;
        }
    }
}
