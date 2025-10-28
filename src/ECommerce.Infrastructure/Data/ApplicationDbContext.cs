using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data
{
    // ApplicationDbContext is the Entity Framework Core context for interacting with the PostgreSQL database.
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet for Users
        public DbSet<User> Users { get; set; }

        // DbSet for Products
        public DbSet<Product> Products { get; set; }

        // DbSet for Orders
        public DbSet<Order> Orders { get; set; }

        // DbSet for OrderItems
        public DbSet<OrderItem> OrderItems { get; set; }

        // Configures the model using Fluent API
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply configurations for each entity
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }
    }
}