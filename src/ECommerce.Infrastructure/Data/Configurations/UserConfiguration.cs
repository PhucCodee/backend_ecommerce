using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.UserId);
            builder.Property(u => u.UserId).HasColumnName("user_id");

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("username");

            builder.Property(u => u.EmailVerified)
                .HasColumnName("email_verified")
                .HasDefaultValue(false);

            builder.Property(u => u.EmailVerifiedAt)
                .HasColumnName("email_verified_at");

            builder.Property(u => u.DeletedAt)
                .HasColumnName("deleted_at");

            builder.Property(u => u.Status)
                .HasColumnName("status")
                .HasColumnType("user_status_enum");

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Indexes
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Username).IsUnique();

            // Relationships
            builder.HasOne(u => u.UserCredential)
                .WithOne(uc => uc.User)
                .HasForeignKey<UserCredential>(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(u => u.UserProfile)
                .WithOne(up => up.User)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}