using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("product_images");

            // Primary key
            builder.HasKey(pi => pi.ImageId);
            builder.Property(pi => pi.ImageId).HasColumnName("image_id");

            // Properties
            builder.Property(pi => pi.ProductId).HasColumnName("product_id");
            builder.Property(pi => pi.SkuId).HasColumnName("sku_id");

            builder.Property(pi => pi.ImageUrl)
                .IsRequired()
                .HasMaxLength(1000)
                .HasColumnName("image_url");

            builder.Property(pi => pi.ThumbnailUrl)
                .HasMaxLength(1000)
                .HasColumnName("thumbnail_url");

            builder.Property(pi => pi.AltText)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("alt_text");

            builder.Property(pi => pi.DisplayOrder)
                .HasColumnName("display_order")
                .HasDefaultValue(0);

            builder.Property(pi => pi.IsPrimary)
                .HasColumnName("is_primary")
                .HasDefaultValue(false);

            builder.Property(pi => pi.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(pi => pi.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pi => pi.Sku)
                .WithMany(ps => ps.ProductImages)
                .HasForeignKey(pi => pi.SkuId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(pi => pi.ProductId);
            builder.HasIndex(pi => pi.SkuId);
            builder.HasIndex(pi => pi.IsPrimary);
            builder.HasIndex(pi => pi.DisplayOrder);
        }
    }
}