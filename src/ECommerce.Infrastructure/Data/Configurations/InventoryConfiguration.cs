using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class InventoryConfiguration : IEntityTypeConfiguration<Inventory>
    {
        public void Configure(EntityTypeBuilder<Inventory> builder)
        {
            builder.ToTable("inventory");

            builder.HasKey(i => i.InventoryId);
            builder.Property(i => i.InventoryId).HasColumnName("inventory_id");

            builder.Property(i => i.SkuId).HasColumnName("sku_id");
            builder.Property(i => i.QuantityAvailable).HasColumnName("quantity_available");
            builder.Property(i => i.QuantityReserved).HasColumnName("quantity_reserved");
            builder.Property(i => i.QuantitySold).HasColumnName("quantity_sold");
            builder.Property(i => i.ReorderPoint).HasColumnName("reorder_point");
            builder.Property(i => i.ReorderQuantity).HasColumnName("reorder_quantity");
            builder.Property(i => i.LastRestockedAt).HasColumnName("last_restocked_at");
            builder.Property(i => i.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Property(i => i.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(i => i.Sku)
                .WithOne(s => s.Inventory)
                .HasForeignKey<Inventory>(i => i.SkuId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}


