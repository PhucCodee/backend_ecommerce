using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class InventoryHistoryConfiguration : IEntityTypeConfiguration<InventoryHistory>
    {
        public void Configure(EntityTypeBuilder<InventoryHistory> builder)
        {
            builder.ToTable("inventory_history");

            // Primary key
            builder.HasKey(ih => ih.HistoryId);
            builder.Property(ih => ih.HistoryId).HasColumnName("history_id");

            // Properties
            builder.Property(ih => ih.InventoryId).HasColumnName("inventory_id");

            builder.Property(ih => ih.ChangeType)
                .HasMaxLength(50)
                .HasColumnName("change_type");

            builder.Property(ih => ih.QuantityChange)
                .HasColumnName("quantity_change");

            builder.Property(ih => ih.QuantityBefore)
                .HasColumnName("quantity_before");

            builder.Property(ih => ih.QuantityAfter)
                .HasColumnName("quantity_after");

            builder.Property(ih => ih.ReferenceType)
                .HasMaxLength(50)
                .HasColumnName("reference_type");

            builder.Property(ih => ih.ReferenceId)
                .HasColumnName("reference_id");

            builder.Property(ih => ih.Notes)
                .HasColumnName("notes");

            builder.Property(ih => ih.ChangedBy)
                .HasColumnName("changed_by");

            builder.Property(ih => ih.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ih => ih.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(ih => ih.Inventory)
                .WithMany(i => i.InventoryHistories)
                .HasForeignKey(ih => ih.InventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ih => ih.ChangedByNavigation)
                .WithMany(u => u.InventoryHistories)
                .HasForeignKey(ih => ih.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(ih => ih.InventoryId);
            builder.HasIndex(ih => ih.ChangeType);
            builder.HasIndex(ih => ih.CreatedAt);
        }
    }
}