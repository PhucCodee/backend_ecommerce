using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ProductMetricConfiguration : IEntityTypeConfiguration<ProductMetric>
    {
        public void Configure(EntityTypeBuilder<ProductMetric> builder)
        {
            builder.ToTable("product_metrics");

            // Primary key
            builder.HasKey(pm => pm.MetricId);
            builder.Property(pm => pm.MetricId).HasColumnName("metric_id");

            // Properties
            builder.Property(pm => pm.ProductId).HasColumnName("product_id");

            builder.Property(pm => pm.Date)
                .HasColumnName("date");

            builder.Property(pm => pm.ViewCount)
                .HasColumnName("view_count")
                .HasDefaultValue(0);

            builder.Property(pm => pm.ClickCount)
                .HasColumnName("click_count")
                .HasDefaultValue(0);

            builder.Property(pm => pm.AddToCartCount)
                .HasColumnName("add_to_cart_count")
                .HasDefaultValue(0);

            builder.Property(pm => pm.PurchaseCount)
                .HasColumnName("purchase_count")
                .HasDefaultValue(0);

            builder.Property(pm => pm.Revenue)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("revenue")
                .HasDefaultValue(0);

            builder.Property(pm => pm.AverageRating)
                .HasColumnType("decimal(3,2)")
                .HasColumnName("average_rating");

            builder.Property(pm => pm.ReviewCount)
                .HasColumnName("review_count")
                .HasDefaultValue(0);

            builder.Property(pm => pm.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(pm => pm.Product)
                .WithMany(p => p.ProductMetrics)
                .HasForeignKey(pm => pm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(pm => new { pm.ProductId, pm.Date }).IsUnique();
            builder.HasIndex(pm => pm.Date);
        }
    }
}