using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts", t =>
        {
            // Check constraint: either user_id OR session_id must exist (XOR)
            t.HasCheckConstraint(
                "cart_owner_check",
                "(user_id IS NOT NULL AND session_id IS NULL) OR (user_id IS NULL AND session_id IS NOT NULL)"
            );
        });

        builder.HasKey(c => c.CartId);
        builder.Property(c => c.CartId)
            .HasColumnName("cart_id");

        builder.Property(c => c.UserId)
            .HasColumnName("user_id");

        builder.Property(c => c.SessionId)
            .HasColumnName("session_id")
            .HasMaxLength(255);

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasColumnType("cart_status_enum");

        builder.Property(c => c.Subtotal)
            .IsRequired()
            .HasColumnType("decimal(10,2)")
            .HasColumnName("subtotal")
            .HasDefaultValue(0m);

        builder.Property(c => c.TotalItems)
            .HasColumnName("total_items")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(c => c.AbandonedAt)
            .HasColumnName("abandoned_at");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(c => c.ExpiresAt)
            .HasColumnName("expires_at");

        // Relationships
        builder.HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.CartItems)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // Partial unique index: only ONE active cart per authenticated user
        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("idx_one_active_cart_per_user")
            .IsUnique()
            .HasFilter("status = 0 AND user_id IS NOT NULL");

        // Partial unique index: only ONE active cart per guest session
        builder.HasIndex(c => c.SessionId)
            .HasDatabaseName("idx_one_active_cart_per_session")
            .IsUnique()
            .HasFilter("status = 0 AND session_id IS NOT NULL");

        // Additional indexes
        builder.HasIndex(c => c.Status)
            .HasDatabaseName("idx_carts_status");

        builder.HasIndex(c => c.ExpiresAt)
            .HasDatabaseName("idx_carts_expires_at");
    }
}