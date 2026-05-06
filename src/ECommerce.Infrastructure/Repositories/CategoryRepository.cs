using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public class CategoryRepository(ApplicationDbContext context) : Repository<Category>(context), ICategoryRepository
    {
        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories.Where(cc => cc.IsActive))
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Category> Categories, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories.Where(cc => cc.IsActive))
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName);

            var totalCount = await query.CountAsync();

            var categories = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }

        public async Task<(IEnumerable<Category> Categories, int TotalCount)> GetCoreCategoriesPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories.Where(cc => cc.IsActive))
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .Where(c => c.IsActive && c.IsCore)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName);

            var totalCount = await query.CountAsync();

            var categories = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (categories, totalCount);
        }

        public override async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories.Where(cc => cc.IsActive))
                .Include(c => c.ProductCategories)
                    .ThenInclude(pc => pc.Product)
                .Where(c => c.IsActive)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Include(c => c.ParentCategory)
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<bool> HasProductsAsync(int categoryId)
        {
            return await _context.ProductCategories
                .AnyAsync(pc => pc.CategoryId == categoryId && pc.Product.RemovedAt == null);
        }
    }
}