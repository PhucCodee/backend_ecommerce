using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllActiveAsync();
        Task<Category?> GetBySlugAsync(string slug);
        Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId);
    }
}



