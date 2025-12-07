using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("coupons");

            builder.HasKey(c => c.CouponId);
            builder.Property(c => c.CouponId).HasColumnName("coupon_id");

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("code");

            builder.Property(c => c.Description)
                .HasColumnName("description");

            builder.Property(c => c.DiscountType)
                .IsRequired()
                .HasColumnName("discount_type");

            builder.Property(c => c.DiscountValue)
                .IsRequired()
                .HasColumnType("decimal(10,2)")
                .HasColumnName("discount_value");

            builder.Property(c => c.MinOrderAmount)
                .HasColumnType("decimal(10,2)")
                .HasColumnName("min_order_amount");

            builder.Property(c => c.UsageLimit)
                .HasColumnName("usage_limit");

            builder.Property(c => c.ValidFrom)
                .HasColumnName("valid_from");

            builder.Property(c => c.ValidUntil)
                .HasColumnName("valid_until");

            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            builder.HasIndex(c => c.Code).IsUnique();

            // Relationships
            builder.HasMany(c => c.Orders)
                .WithOne(o => o.Coupon)
                .HasForeignKey(o => o.CouponId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}