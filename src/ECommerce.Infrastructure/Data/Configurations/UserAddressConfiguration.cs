using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserAddressConfiguration : IEntityTypeConfiguration<UserAddress>
    {
        public void Configure(EntityTypeBuilder<UserAddress> builder)
        {
            builder.ToTable("user_addresses");

            // Primary key
            builder.HasKey(ua => ua.AddressId);
            builder.Property(ua => ua.AddressId).HasColumnName("address_id");

            // Properties
            builder.Property(ua => ua.UserId).HasColumnName("user_id");

            builder.Property(ua => ua.Type)
                .HasColumnName("address_type")
                .HasColumnType("address_type_enum");

            builder.Property(ua => ua.IsDefaultShipping)
                .HasColumnName("is_default_shipping")
                .HasDefaultValue(false);

            builder.Property(ua => ua.IsDefaultBilling)
                .HasColumnName("is_default_billing")
                .HasDefaultValue(false);

            builder.Property(ua => ua.Label)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("label");

            builder.Property(ua => ua.RecipientName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("recipient_name");

            builder.Property(ua => ua.Phone)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("phone");

            builder.Property(ua => ua.AddressLine1)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address_line1");

            builder.Property(ua => ua.AddressLine2)
                .HasMaxLength(255)
                .HasColumnName("address_line2");

            builder.Property(ua => ua.City)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("city");

            builder.Property(ua => ua.StateProvince)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("state_province");

            builder.Property(ua => ua.PostalCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnName("postal_code");

            builder.Property(ua => ua.Country)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("country");

            builder.Property(ua => ua.Latitude)
                .HasColumnName("latitude");

            builder.Property(ua => ua.Longitude)
                .HasColumnName("longitude");

            builder.Property(ua => ua.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ua => ua.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(ua => ua.User)
                .WithMany(u => u.UserAddresses)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(ua => ua.UserId);
            builder.HasIndex(ua => ua.IsDefaultShipping);
            builder.HasIndex(ua => ua.IsDefaultBilling);
            builder.HasIndex(ua => ua.City);
            builder.HasIndex(ua => ua.Country);
        }
    }
}