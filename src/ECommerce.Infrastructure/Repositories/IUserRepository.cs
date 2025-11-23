using ECommerce.Domain.Entities;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetUserWithCredentialsAsync(string email);
        Task<User?> GetUserWithProfileAsync(int userId);
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
    }
}