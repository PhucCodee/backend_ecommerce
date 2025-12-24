using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("cart_items");

        builder.HasKey(ci => ci.CartItemId);
        builder.Property(ci => ci.CartItemId)
            .HasColumnName("cart_item_id");

        builder.Property(ci => ci.CartId)
            .HasColumnName("cart_id")
            .IsRequired();

        builder.Property(ci => ci.SkuId)
            .HasColumnName("sku_id")
            .IsRequired();

        builder.Property(ci => ci.Quantity)
            .HasColumnName("quantity")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(ci => ci.PriceSnapshot)
            .HasColumnName("price_snapshot")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(ci => ci.AddedAt)
            .HasColumnName("added_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(ci => ci.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Relationships
        builder.HasOne(ci => ci.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Sku)
            .WithMany(ps => ps.CartItems)
            .HasForeignKey(ci => ci.SkuId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}