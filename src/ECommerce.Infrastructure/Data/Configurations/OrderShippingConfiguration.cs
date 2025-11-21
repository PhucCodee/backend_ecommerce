using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderShippingConfiguration : IEntityTypeConfiguration<OrderShipping>
    {
        public void Configure(EntityTypeBuilder<OrderShipping> builder)
        {
            builder.ToTable("order_shipping");

            // Primary key
            builder.HasKey(os => os.ShippingId);
            builder.Property(os => os.ShippingId).HasColumnName("shipping_id");

            // Properties
            builder.Property(os => os.OrderId).HasColumnName("order_id");

            builder.Property(os => os.RecipientName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("recipient_name");

            builder.Property(os => os.Phone)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("phone");

            builder.Property(os => os.AddressLine1)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("address_line_1");

            builder.Property(os => os.AddressLine2)
                .HasMaxLength(500)
                .HasColumnName("address_line_2");

            builder.Property(os => os.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("city");

            builder.Property(os => os.StateProvince)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("state_province");

            builder.Property(os => os.PostalCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("postal_code");

            builder.Property(os => os.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("country");

            builder.Property(os => os.Method)
                .HasConversion<string>()
                .HasColumnName("method");

            builder.Property(os => os.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(os => os.Order)
                .WithOne(o => o.OrderShipping)
                .HasForeignKey<OrderShipping>(os => os.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(os => os.OrderId).IsUnique();
            builder.HasIndex(os => os.RecipientName);
            builder.HasIndex(os => os.Phone);
            builder.HasIndex(os => os.PostalCode);
            builder.HasIndex(os => os.Country);
        }
    }
}