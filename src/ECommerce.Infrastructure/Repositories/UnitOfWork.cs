using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Data;

namespace ECommerce.Infrastructure.Repositories
{
    public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
    {
        private readonly ApplicationDbContext _context = context;
        private IUserRepository? _users;
        private IProductRepository? _products;
        private IOrderRepository? _orders;
        private IRepository<Category>? _categories;
        private IRepository<CartItem>? _cartItems;
        private IRepository<UserAddress>? _userAddresses;
        private IRepository<Review>? _reviews;
        private IOrderPaymentRepository? _orderPayments;
        private IInventoryRepository? _inventories;
        private IProductSkuRepository? _productSkus;
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IRepository<Category> Categories =>
            _categories ??= new Repository<Category>(_context);
        public IRepository<CartItem> CartItems => _cartItems ??= new Repository<CartItem>(_context);
        public IRepository<UserAddress> UserAddresses =>
            _userAddresses ??= new Repository<UserAddress>(_context);
        public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context);
        public IOrderPaymentRepository OrderPayments =>
            _orderPayments ??= new OrderPaymentRepository(_context);
        public IInventoryRepository Inventories =>
            _inventories ??= new InventoryRepository(_context);
        public IProductSkuRepository ProductSkus =>
            _productSkus ??= new ProductSkuRepository(_context);

        public async Task<int> SaveChangesAsync()
        {
            // Update timestamps for entities
            foreach (var entry in _context.ChangeTracker.Entries())
            {
                if (entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            CancellationToken cancellationToken = default
        )
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(
                cancellationToken
            );
            try
            {
                var result = await action();
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
