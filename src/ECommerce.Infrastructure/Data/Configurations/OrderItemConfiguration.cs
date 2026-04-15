using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.ToTable("order_items");

            builder.HasKey(oi => oi.OrderItemId);
            builder.Property(oi => oi.OrderItemId).HasColumnName("order_item_id");
            builder.Property(oi => oi.OrderId).HasColumnName("order_id");
            builder.Property(oi => oi.SkuId).HasColumnName("sku_id");
            builder.Property(oi => oi.ProductName).HasColumnName("product_name").HasMaxLength(255).IsRequired();
            builder.Property(oi => oi.Sku).HasColumnName("sku").HasMaxLength(100).IsRequired();
            builder.Property(oi => oi.VariantDescription).HasColumnName("variant_description");
            builder.Property(oi => oi.SellerId).HasColumnName("seller_id");
            builder.Property(oi => oi.Quantity).HasColumnName("quantity");
            builder.Property(oi => oi.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(10,2)");
            builder.Property(oi => oi.Subtotal).HasColumnName("subtotal").HasColumnType("decimal(10,2)");
            builder.Property(oi => oi.CreatedAt).HasColumnName("created_at");

            builder.HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(oi => oi.Seller)
                .WithMany(u => u.OrderItems)
                .HasForeignKey(oi => oi.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(oi => oi.SkuNavigation)
                .WithMany(ps => ps.OrderItems)
                .HasForeignKey(oi => oi.SkuId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(oi => oi.OrderId);
            builder.HasIndex(oi => oi.SellerId);
        }
    }
}