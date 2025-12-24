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
        public async Task<IEnumerable<Category>> GetAllActiveAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task<IEnumerable<Category>> GetChildCategoriesAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentCategoryId == parentId && c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        public override async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public override async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}



