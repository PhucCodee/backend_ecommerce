using System;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using System.Threading;

namespace ECommerce.Domain.Repositories
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
        Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default);
    }
}