using System;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IRepository<Category> Categories { get; }
        IRepository<CartItem> CartItems { get; }
        IRepository<UserAddress> UserAddresses { get; }
        IRepository<Review> Reviews { get; }

        Task<int> SaveChangesAsync();
    }
}