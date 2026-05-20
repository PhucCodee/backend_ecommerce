using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId);
        Task<(IEnumerable<Category> Categories, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize,
            bool includeInactive = false
        );
        Task<(IEnumerable<Category> Categories, int TotalCount)> GetCoreCategoriesPagedAsync(
            int pageNumber,
            int pageSize
        );
        Task<bool> HasProductsAsync(int categoryId);
    }
}
