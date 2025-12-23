using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.ToTable("reviews");

            builder.HasKey(r => r.ReviewId);
            builder.Property(r => r.ReviewId).HasColumnName("review_id");

            builder.Property(r => r.ProductId).HasColumnName("product_id");
            builder.Property(r => r.UserId).HasColumnName("user_id");
            builder.Property(r => r.OrderItemId).HasColumnName("order_item_id");
            builder.Property(r => r.ModeratedBy).HasColumnName("moderated_by");

            builder.HasOne(r => r.User) // Reviewer
                .WithMany(u => u.ReviewUsers)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.ModeratedByNavigation) // Moderator
                .WithMany(u => u.ReviewModeratedByNavigations)
                .HasForeignKey(r => r.ModeratedBy)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(r => r.OrderItem)
                .WithMany(o => o.Reviews)
                .HasForeignKey(r => r.OrderItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            builder.Property(r => r.Comment)
                .HasColumnName("comment");

            builder.Property(r => r.ModerationNotes)
                .HasColumnName("moderation_notes");

            builder.Property(r => r.Rating)
                .HasColumnName("rating");

            builder.Property(r => r.IsVerifiedPurchase)
                .HasColumnName("is_verified_purchase")
                .HasDefaultValue(true);

            builder.Property(r => r.IsApproved)
                .HasColumnName("is_approved")
                .HasDefaultValue(true);

            builder.Property(r => r.HelpfulCount)
                .HasColumnName("helpful_count")
                .HasDefaultValue(0);

            builder.Property(r => r.UnhelpfulCount)
                .HasColumnName("unhelpful_count")
                .HasDefaultValue(0);

            builder.Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(r => r.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasIndex(r => new { r.ProductId, r.UserId, r.OrderItemId })
                .IsUnique();

            // Other indexes
            builder.HasIndex(r => r.ProductId);
            builder.HasIndex(r => r.UserId);
            builder.HasIndex(r => r.Rating);
        }
    }
}