using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class UserItemInteractionConfiguration : IEntityTypeConfiguration<UserItemInteraction>
    {
        public void Configure(EntityTypeBuilder<UserItemInteraction> builder)
        {
            builder.ToTable("user_item_interactions");

            // Primary key
            builder.HasKey(uii => uii.InteractionId);
            builder.Property(uii => uii.InteractionId).HasColumnName("interaction_id");

            // Properties
            builder.Property(uii => uii.UserId).HasColumnName("user_id");
            builder.Property(uii => uii.ProductId).HasColumnName("product_id");

            builder.Property(uii => uii.InteractionType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("interaction_type");

            builder.Property(uii => uii.Weight)
                .HasColumnName("weight")
                .HasDefaultValue(1);

            builder.Property(uii => uii.SessionId)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("session_id");

            builder.Property(uii => uii.ReferrerUrl)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("referrer_url");

            builder.Property(uii => uii.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(uii => uii.User)
                .WithMany(u => u.UserItemInteractions)
                .HasForeignKey(uii => uii.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(uii => uii.Product)
                .WithMany(p => p.UserItemInteractions)
                .HasForeignKey(uii => uii.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(uii => uii.UserId);
            builder.HasIndex(uii => uii.ProductId);
            builder.HasIndex(uii => uii.InteractionType);
            builder.HasIndex(uii => uii.SessionId);
            builder.HasIndex(uii => uii.CreatedAt);
        }
    }
}