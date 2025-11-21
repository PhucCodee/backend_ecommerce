using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("user_roles");

            builder.HasKey(ur => ur.UserRoleId);
            builder.Property(ur => ur.UserRoleId).HasColumnName("user_role_id");

            builder.Property(ur => ur.UserId).HasColumnName("user_id");
            builder.Property(ur => ur.GrantedBy).HasColumnName("granted_by");

            builder.Property(ur => ur.Role)
                .HasConversion<string>()
                .HasColumnName("role");

            builder.Property(ur => ur.GrantedAt)
                .HasColumnName("granted_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ur => ur.RevokedAt)
                .HasColumnName("revoked_at");

            builder.HasOne(ur => ur.User) // User who has the role
                .WithMany(u => u.UserRoleUsers)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.GrantedByNavigation) // Admin who granted the role
                .WithMany(u => u.UserRoleGrantedByNavigation)
                .HasForeignKey(ur => ur.GrantedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(ur => ur.UserId);
            builder.HasIndex(ur => ur.GrantedBy);
            builder.HasIndex(ur => ur.Role);

            // Unique constraint: one role per user
            builder.HasIndex(ur => new { ur.UserId, ur.Role })
                .IsUnique()
                .HasFilter("revoked_at IS NULL"); // Only active roles should be unique
        }
    }
}