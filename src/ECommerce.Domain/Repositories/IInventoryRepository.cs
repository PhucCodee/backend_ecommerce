using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;

namespace ECommerce.Domain.Repositories
{
    public interface IInventoryRepository : IRepository<Inventory>
    {
        Task<bool> TryReserveStockAsync(int skuId, int quantity);
        Task<bool> TryReleaseReservedStockAsync(int skuId, int quantity);
    }
}
