using Microsoft.EntityFrameworkCore;
using ECommerce.Domain.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserCredential> UserCredentials { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; }
        public DbSet<UserItemInteraction> UserItemInteractions { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSku> ProductSkus { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductMetric> ProductMetrics { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryHistory> InventoryHistories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderShipping> OrderShippings { get; set; }
        public DbSet<OrderPayment> OrderPayments { get; set; }
        public DbSet<OrderFulfillment> OrderFulfillments { get; set; }
        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<EventLog> EventLogs { get; set; }
        public DbSet<ProcessedEvent> ProcessedEvents { get; set; }
        public DbSet<DeadLetterQueue> DeadLetterQueues { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasPostgresEnum<AddressType>()
                .HasPostgresEnum<CartStatus>()
                .HasPostgresEnum<Currency>()
                .HasPostgresEnum<DeviceType>()
                .HasPostgresEnum<EntityType>()
                .HasPostgresEnum<EventStatus>()
                .HasPostgresEnum<Language>()
                .HasPostgresEnum<LoginStatus>()
                .HasPostgresEnum<ModerationStatus>()
                .HasPostgresEnum<NotificationChannel>()
                .HasPostgresEnum<NotificationPriority>()
                .HasPostgresEnum<NotificationStatus>()
                .HasPostgresEnum<OrderStatus>()
                .HasPostgresEnum<PaymentMethod>()
                .HasPostgresEnum<PaymentStatus>()
                .HasPostgresEnum<ProductStatus>()
                .HasPostgresEnum<ShippingMethod>()
                .HasPostgresEnum<UserGender>()
                .HasPostgresEnum<UserRoleType>()
                .HasPostgresEnum<UserStatus>();

            // Apply entity configurations from separate files
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

    }
}