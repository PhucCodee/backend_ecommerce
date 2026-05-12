using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories
{
    public class InventoryRepository(ApplicationDbContext context)
        : Repository<Inventory>(context),
            IInventoryRepository { }
}
