using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ReviewImageConfiguration : IEntityTypeConfiguration<ReviewImage>
    {
        public void Configure(EntityTypeBuilder<ReviewImage> builder)
        {
            builder.ToTable("review_images");

            // Primary key
            builder.HasKey(ri => ri.ImageId);
            builder.Property(ri => ri.ImageId).HasColumnName("image_id");

            // Properties
            builder.Property(ri => ri.ReviewId).HasColumnName("review_id");

            builder.Property(ri => ri.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("image_url");

            builder.Property(ri => ri.ThumbnailUrl)
                .HasMaxLength(1000)
                .HasColumnName("thumbnail_url");

            builder.Property(ri => ri.DisplayOrder)
                .HasColumnName("display_order")
                .HasDefaultValue(0);

            builder.Property(ri => ri.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(ri => ri.Review)
                .WithMany(r => r.ReviewImages)
                .HasForeignKey(ri => ri.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(ri => ri.ReviewId);
            builder.HasIndex(ri => ri.DisplayOrder);
        }
    }
}