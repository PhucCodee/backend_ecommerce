using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<Product>> GetProductsByCategoryNameAsync(string categoryName);
        Task<IEnumerable<Product>> GetAllWithDetailsAsync();
        Task<Product?> GetByIdWithDetailsAsync(int id);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<Product> Products, int TotalCount)> GetBySellerPagedAsync(int sellerId, int pageNumber, int pageSize);
        Task<bool> SlugExistsAsync(string slug, int? excludeProductId = null);

        Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredPagedAsync(
            int pageNumber,
            int pageSize,
            string? sortBy = null,
            bool desc = false,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            int? categoryId = null,
            string? brand = null,
            int? sellerId = null,
            string? status = null,
            string? search = null,
            bool primaryOnly = true,
            bool? inStock = null);
    }
}