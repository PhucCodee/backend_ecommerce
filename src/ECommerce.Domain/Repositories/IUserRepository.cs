using ECommerce.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername);
        Task<User?> GetUserWithCredentialsAsync(string email);
        Task<User?> GetUserWithProfileAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<IEnumerable<User>> GetAllWithProfileAsync();
        Task<User?> GetWithProfileAsync(int userId);
    }
}