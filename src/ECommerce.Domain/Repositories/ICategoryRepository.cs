using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllActiveAsync();
        Task<Category?> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId);
        Task<(IEnumerable<Category> Categories, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}