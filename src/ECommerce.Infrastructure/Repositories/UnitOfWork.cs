using ECommerce.Infrastructure.Data;
using ECommerce.Domain.Entities;
using System.Threading.Tasks;
using System;
using ECommerce.Domain.Repositories;

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

        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
        public IRepository<CartItem> CartItems => _cartItems ??= new Repository<CartItem>(_context);
        public IRepository<UserAddress> UserAddresses => _userAddresses ??= new Repository<UserAddress>(_context);
        public IRepository<Review> Reviews => _reviews ??= new Repository<Review>(_context);

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

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}