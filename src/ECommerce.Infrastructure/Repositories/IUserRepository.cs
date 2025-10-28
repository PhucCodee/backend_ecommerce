using ECommerce.Domain.Entities;
using System.Threading.Tasks;

namespace ECommerce.Infrastructure.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByEmailAsync(string email);
    }
}