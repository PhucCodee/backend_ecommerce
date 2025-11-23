using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserLoginHistoryConfiguration : IEntityTypeConfiguration<UserLoginHistory>
    {
        public void Configure(EntityTypeBuilder<UserLoginHistory> builder)
        {
            builder.ToTable("user_login_history");

            // Primary key
            builder.HasKey(ulh => ulh.LoginId);
            builder.Property(ulh => ulh.LoginId).HasColumnName("login_id");

            // Properties
            builder.Property(ulh => ulh.UserId).HasColumnName("user_id");

            builder.Property(ulh => ulh.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");

            builder.Property(ulh => ulh.Status)
                .HasColumnName("login_status")
                .HasColumnType("login_status_enum");

            builder.Property(ulh => ulh.FailureReason)
                .HasMaxLength(500)
                .HasColumnName("failure_reason");

            builder.Property(ulh => ulh.IpAddress)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("ip_address");

            builder.Property(ulh => ulh.UserAgent)
                .HasMaxLength(1000)
                .HasColumnName("user_agent");

            builder.Property(ulh => ulh.LocationCountry)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("location_country");

            builder.Property(ulh => ulh.LocationCity)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("location_city");

            builder.Property(ulh => ulh.IsSuspicious)
                .HasColumnName("is_suspicious")
                .HasDefaultValue(false);

            builder.Property(ulh => ulh.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(ulh => ulh.User)
                .WithMany(u => u.UserLoginHistories)
                .HasForeignKey(ulh => ulh.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(ulh => ulh.UserId);
            builder.HasIndex(ulh => ulh.Email);
            builder.HasIndex(ulh => ulh.Status);
            builder.HasIndex(ulh => ulh.IsSuspicious);
            builder.HasIndex(ulh => ulh.CreatedAt);
        }
    }
}