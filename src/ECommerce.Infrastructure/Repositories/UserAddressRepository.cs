using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories
{
    public class UserAddressRepository(ApplicationDbContext context)
        : Repository<UserAddress>(context),
            IUserAddressRepository
    {
        public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(int userId)
        {
            return await _context
                .UserAddresses.Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefaultShipping || a.IsDefaultBilling)
                .ThenByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserAddress?> GetByIdAndUserIdAsync(int addressId, int userId)
        {
            return await _context.UserAddresses.FirstOrDefaultAsync(a =>
                a.AddressId == addressId && a.UserId == userId
            );
        }

        public async Task ClearDefaultShippingAsync(int userId, int? excludeAddressId = null)
        {
            var query = _context.UserAddresses.Where(a =>
                a.UserId == userId && a.IsDefaultShipping
            );
            if (excludeAddressId.HasValue)
                query = query.Where(a => a.AddressId != excludeAddressId.Value);

            var addresses = await query.ToListAsync();
            foreach (var address in addresses)
                address.IsDefaultShipping = false;
        }

        public async Task ClearDefaultBillingAsync(int userId, int? excludeAddressId = null)
        {
            var query = _context.UserAddresses.Where(a => a.UserId == userId && a.IsDefaultBilling);
            if (excludeAddressId.HasValue)
                query = query.Where(a => a.AddressId != excludeAddressId.Value);

            var addresses = await query.ToListAsync();
            foreach (var address in addresses)
                address.IsDefaultBilling = false;
        }

        public async Task<UserAddress?> GetFirstByUserIdAsync(
            int userId,
            int? excludeAddressId = null
        )
        {
            var query = _context.UserAddresses.Where(a => a.UserId == userId);
            if (excludeAddressId.HasValue)
                query = query.Where(a => a.AddressId != excludeAddressId.Value);

            return await query.OrderBy(a => a.AddressId).FirstOrDefaultAsync();
        }

        public async Task DeleteAsync(UserAddress address)
        {
            _context.UserAddresses.Remove(address);
            await Task.CompletedTask;
        }
    }
}
