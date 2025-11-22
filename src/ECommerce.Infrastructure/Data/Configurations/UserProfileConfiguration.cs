using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
    {
        public void Configure(EntityTypeBuilder<UserProfile> builder)
        {
            builder.ToTable("user_profiles");

            // Primary key
            builder.HasKey(up => up.ProfileId);
            builder.Property(up => up.ProfileId).HasColumnName("profile_id");

            // Properties
            builder.Property(up => up.UserId).HasColumnName("user_id");

            builder.Property(up => up.FirstName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("first_name");

            builder.Property(up => up.LastName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("last_name");

            builder.Property(up => up.Phone)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("phone");

            builder.Property(up => up.DateOfBirth)
                .HasColumnName("date_of_birth");

            builder.Property(up => up.Gender)
                .HasColumnName("gender").HasColumnType("user_gender_enum");

            builder.Property(up => up.AvatarUrl)
                .HasMaxLength(500)
                .HasColumnName("avatar_url");

            builder.Property(up => up.Bio)
                .HasMaxLength(1000)
                .HasColumnName("bio");

            builder.Property(up => up.PreferredLanguage)
                .HasColumnName("language")
                .HasColumnType("language_enum");

            builder.Property(up => up.PreferredCurrency)
                .HasColumnName("currency")
                .HasColumnType("currency_enum");

            builder.Property(up => up.Timezone)
                .HasMaxLength(100)
                .HasColumnName("timezone");

            builder.Property(up => up.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(up => up.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(up => up.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(up => up.UserId).IsUnique();
            builder.HasIndex(up => up.Phone);
        }
    }
}