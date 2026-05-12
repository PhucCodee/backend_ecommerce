using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class UserRepository(ApplicationDbContext context)
        : Repository<User>(context),
            IUserRepository
    {
        private IQueryable<User> GetActiveUsers() => _context.Users.Where(u => u.DeletedAt == null);

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await GetActiveUsers().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await GetActiveUsers().FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername)
        {
            return await GetActiveUsers()
                .Include(u => u.UserCredential)
                .Include(u => u.UserProfile)
                .Include(u => u.UserRoleUsers.Where(r => r.RevokedAt == null))
                .FirstOrDefaultAsync(u =>
                    u.Email == emailOrUsername || u.Username == emailOrUsername
                );
        }

        public async Task<User?> GetUserWithCredentialsAsync(string email)
        {
            return await GetActiveUsers()
                .Include(u => u.UserCredential)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserWithProfileAsync(int userId)
        {
            return await GetActiveUsers()
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithRolesAsync(int userId)
        {
            return await GetActiveUsers()
                .Include(u => u.UserRoleUsers.Where(r => r.RevokedAt == null))
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<User?> GetUserWithAllDetailsAsync(int userId)
        {
            return await GetActiveUsers()
                .Include(u => u.UserCredential)
                .Include(u => u.UserProfile)
                .Include(u => u.UserRoleUsers.Where(r => r.RevokedAt == null))
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await GetActiveUsers().AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await GetActiveUsers().AnyAsync(u => u.Username == username);
        }

        public async Task<IEnumerable<User>> GetAllWithProfileAsync()
        {
            return await GetActiveUsers().Include(u => u.UserProfile).ToListAsync();
        }

        public async Task<User?> GetWithProfileAsync(int userId)
        {
            return await GetActiveUsers()
                .Include(u => u.UserProfile)
                .Include(u => u.UserCredential)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await GetActiveUsers().ToListAsync();
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(
            int pageNumber,
            int pageSize
        )
        {
            var query = GetActiveUsers()
                .Include(u => u.UserProfile)
                .Include(u => u.UserRoleUsers.Where(r => r.RevokedAt == null));

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.UserId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}
