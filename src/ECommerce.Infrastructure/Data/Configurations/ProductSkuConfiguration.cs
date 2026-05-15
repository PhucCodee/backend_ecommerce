using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ProductSkuConfiguration : IEntityTypeConfiguration<ProductSku>
    {
        public void Configure(EntityTypeBuilder<ProductSku> builder)
        {
            builder.ToTable("product_skus");

            // Primary key
            builder.HasKey(ps => ps.SkuId);
            builder.Property(ps => ps.SkuId).HasColumnName("sku_id");

            // Properties
            builder.Property(ps => ps.ProductId).HasColumnName("product_id");

            builder.Property(ps => ps.Sku).IsRequired().HasMaxLength(100).HasColumnName("sku");

            builder.Property(ps => ps.Color).HasMaxLength(50).HasColumnName("color");
            builder.Property(ps => ps.Size).HasMaxLength(10).HasColumnName("size");

            builder.Property(ps => ps.Price).HasColumnType("decimal(18,2)").HasColumnName("price");

            builder
                .Property(ps => ps.CostPrice)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("cost_price");

            builder
                .Property(ps => ps.CompareAtPrice)
                .HasColumnType("decimal(18,2)")
                .HasColumnName("compare_at_price");

            builder.Property(ps => ps.IsActive).HasColumnName("is_active").HasDefaultValue(true);

            builder.Property(ps => ps.IsDefault).HasColumnName("is_default").HasDefaultValue(false);

            builder
                .Property(ps => ps.WeightKg)
                .HasColumnType("decimal(10,2)")
                .HasColumnName("weight_kg");

            builder.Property(ps => ps.DimensionsCm).HasMaxLength(50).HasColumnName("dimensions_cm");

            builder
                .Property(ps => ps.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder
                .Property(ps => ps.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSkus)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(ps => ps.Inventory)
                .WithOne(i => i.Sku)
                .HasForeignKey<Inventory>(i => i.SkuId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(ps => ps.ProductId);
            builder.HasIndex(ps => ps.Sku).IsUnique();
            builder.HasIndex(ps => ps.IsActive);
            builder.HasIndex(ps => ps.IsDefault);
        }
    }
}
