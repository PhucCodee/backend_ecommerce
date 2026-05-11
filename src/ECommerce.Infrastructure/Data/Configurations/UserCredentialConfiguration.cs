using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserCredentialConfiguration : IEntityTypeConfiguration<UserCredential>
    {
        public void Configure(EntityTypeBuilder<UserCredential> builder)
        {
            builder.ToTable("user_credentials");

            // Primary key
            builder.HasKey(uc => uc.CredentialId);
            builder.Property(uc => uc.CredentialId).HasColumnName("credential_id");

            // Properties
            builder.Property(uc => uc.UserId).HasColumnName("user_id");

            builder
                .Property(uc => uc.PasswordHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password_hash");

            builder.Property(uc => uc.PasswordUpdatedAt).HasColumnName("password_updated_at");

            builder
                .Property(uc => uc.FailedLoginAttempts)
                .HasColumnName("failed_login_attempts")
                .HasDefaultValue(0);

            builder.Property(uc => uc.LastFailedAttemptAt).HasColumnName("last_failed_attempt_at");

            builder.Property(uc => uc.LockedUntil).HasColumnName("locked_until");

            builder.Property(uc => uc.LastLoginAt).HasColumnName("last_login_at");

            builder
                .Property(uc => uc.LastLoginIp)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("last_login_ip");

            builder
                .Property(uc => uc.ResetTokenHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("reset_token_hash");

            builder.Property(uc => uc.ResetTokenExpiresAt).HasColumnName("reset_token_expires_at");

            builder
                .Property(uc => uc.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder
                .Property(uc => uc.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder
                .HasOne(uc => uc.User)
                .WithOne(u => u.UserCredential)
                .HasForeignKey<UserCredential>(uc => uc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(uc => uc.UserId).IsUnique();
            builder.HasIndex(uc => uc.ResetTokenHash);
            builder.HasIndex(uc => uc.LastLoginAt);
        }
    }
}
