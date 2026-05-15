using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
        Task<User?> GetUserWithProfileAsync(int userId);
        Task<User?> GetUserWithAllDetailsAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    }
}
