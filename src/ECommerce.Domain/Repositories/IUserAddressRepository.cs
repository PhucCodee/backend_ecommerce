using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface IUserAddressRepository : IRepository<UserAddress>
    {
        Task<IEnumerable<UserAddress>> GetByUserIdAsync(int userId);
        Task<UserAddress?> GetByIdAndUserIdAsync(int addressId, int userId);
        Task ClearDefaultShippingAsync(int userId, int? excludeAddressId = null);
        Task ClearDefaultBillingAsync(int userId, int? excludeAddressId = null);
        Task<UserAddress?> GetFirstByUserIdAsync(int userId, int? excludeAddressId = null);
        Task DeleteAsync(UserAddress address);
    }
}