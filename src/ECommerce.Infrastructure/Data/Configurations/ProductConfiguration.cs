using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("products");

            builder.HasKey(p => p.ProductId);
            builder.Property(p => p.ProductId).HasColumnName("product_id");

            builder.Property(p => p.SellerId).HasColumnName("seller_id");
            builder.Property(p => p.CategoryId).HasColumnName("category_id");

            builder.Property(p => p.ProductName)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("product_name");

            builder.Property(p => p.Slug)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("slug");

            builder.Property(p => p.BaseSku)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("base_sku");

            builder.Property(p => p.Description)
                .HasColumnName("description");

            builder.Property(p => p.ShortDescription)
                .HasMaxLength(500)
                .HasColumnName("short_description");

            builder.Property(p => p.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");

            builder.Property(p => p.DimensionsCm)
                .HasMaxLength(50)
                .HasColumnName("dimensions_cm");

            builder.Property(p => p.MetaTitle)
                .HasMaxLength(200)
                .HasColumnName("meta_title");

            builder.Property(p => p.MetaDescription)
                .HasMaxLength(500)
                .HasColumnName("meta_description");

            builder.Property(p => p.Tags)
                .HasColumnName("tags");

            builder.Property(p => p.HasVariants)
                .HasColumnName("has_variants")
                .HasDefaultValue(false);

            builder.Property(p => p.WeightKg)
                .HasColumnType("decimal(10,2)")
                .HasColumnName("weight_kg");

            builder.Property(p => p.ViewCount)
                .HasColumnName("view_count")
                .HasDefaultValue(0);

            builder.Property(p => p.Status)
                .HasColumnName("status")
                .HasColumnType("product_status_enum");

            builder.Property(p => p.Moderation)
                .HasColumnName("moderation_status")
                .HasColumnType("moderation_status_enum");

            builder.Property(p => p.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(p => p.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(p => p.PublishedAt)
                .HasColumnName("published_at");

            builder.Property(p => p.RemovedAt)
                .HasColumnName("removed_at");

            // Indexes
            builder.HasIndex(p => p.Slug).IsUnique();
            builder.HasIndex(p => p.BaseSku).IsUnique();
            builder.HasIndex(p => p.SellerId);
            builder.HasIndex(p => p.CategoryId);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.ProductName);
            builder.HasIndex(p => p.Brand);

            // Relationships
            builder.HasOne(p => p.Seller)
                .WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.ProductSkus)
                .WithOne(ps => ps.Product)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.ProductMetrics)
                .WithOne(pm => pm.Product)
                .HasForeignKey(pm => pm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}