using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("orders");

            builder.HasKey(o => o.OrderId);
            builder.Property(o => o.OrderId).HasColumnName("order_id");

            builder.Property(o => o.OrderNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("order_number");

            builder.Property(o => o.UserId)
                .HasColumnName("user_id");

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasColumnName("status")
                .HasDefaultValue(OrderStatus.Created);

            builder.Property(o => o.PreferredCurrency)
                .HasConversion<string>()
                .HasColumnName("preferred_currency")
                .HasDefaultValue(Currency.VND);

            builder.Property(o => o.Subtotal)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("subtotal");

            builder.Property(o => o.ShippingFee)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("shipping_fee")
                .HasDefaultValue(0);

            builder.Property(o => o.TaxAmount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("tax_amount")
                .HasDefaultValue(0);

            builder.Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("discount_amount")
                .HasDefaultValue(0);

            builder.Property(o => o.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("total_amount");

            builder.Property(o => o.CustomerNotes)
                .HasColumnName("customer_notes");

            builder.Property(o => o.AdminNotes)
                .HasColumnName("admin_notes");

            builder.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(o => o.CancelledAt)
                .HasColumnName("cancelled_at");

            // Indexes
            builder.HasIndex(o => o.OrderNumber).IsUnique();
            builder.HasIndex(o => o.UserId);
            builder.HasIndex(o => o.Status);

            // Relationships
            builder.HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.OrderPayments)
                .WithOne(op => op.Order)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(o => o.OrderStatusHistories)
                .WithOne(osh => osh.Order)
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.OrderShipping)
                .WithOne(os => os.Order)
                .HasForeignKey<OrderShipping>(os => os.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.OrderFulfillment)
                .WithOne(of => of.Order)
                .HasForeignKey<OrderFulfillment>(of => of.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}