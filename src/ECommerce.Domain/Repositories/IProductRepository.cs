using ECommerce.Domain.Entities;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<Product?> GetByIdIncludingRemovedAsync(int id);
        Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null);
    }
}