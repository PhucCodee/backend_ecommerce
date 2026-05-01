using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;

namespace ECommerce.Domain.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IOrderPaymentRepository OrderPayments { get; }
        IInventoryRepository Inventories { get; }
        IProductSkuRepository ProductSkus { get; }
        IRepository<Category> Categories { get; }
        IRepository<CartItem> CartItems { get; }
        IRepository<UserAddress> UserAddresses { get; }
        IRepository<Review> Reviews { get; }

        Task<int> SaveChangesAsync();
        Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            CancellationToken cancellationToken = default
        );
    }
}