using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.ToTable("user_sessions");

            // Primary key
            builder.HasKey(us => us.SessionId);
            builder.Property(us => us.SessionId).HasColumnName("session_id");

            // Properties
            builder.Property(us => us.UserId).HasColumnName("user_id");

            builder.Property(us => us.AccessTokenHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("access_token_hash");

            builder.Property(us => us.RefreshTokenHash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("refresh_token_hash");

            builder.Property(us => us.IpAddress)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("ip_address");

            builder.Property(us => us.UserAgent)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("user_agent");

            builder.Property(us => us.Type)
                .HasConversion<string>()
                .HasColumnName("type");

            builder.Property(us => us.DeviceName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("device_name");

            builder.Property(us => us.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(us => us.LastActivityAt)
                .HasColumnName("last_activity_at");

            builder.Property(us => us.ExpiresAt)
                .HasColumnName("expires_at");

            builder.Property(us => us.RevokedAt)
                .HasColumnName("revoked_at");

            // Relationships
            builder.HasOne(us => us.User)
                .WithMany(u => u.UserSessions)
                .HasForeignKey(us => us.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(us => us.UserId);
            builder.HasIndex(us => us.AccessTokenHash).IsUnique();
            builder.HasIndex(us => us.RefreshTokenHash).IsUnique();
            builder.HasIndex(us => us.ExpiresAt);
        }
    }
}