using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
    {
        public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
        {
            builder.ToTable("order_status_history");

            // Primary key
            builder.HasKey(osh => osh.HistoryId);
            builder.Property(osh => osh.HistoryId).HasColumnName("history_id");

            // Properties
            builder.Property(osh => osh.OrderId).HasColumnName("order_id");

            builder.Property(osh => osh.OldStatus)
                .HasConversion<string>()
                .HasColumnName("old_status");

            builder.Property(osh => osh.NewStatus)
                .HasConversion<string>()
                .HasColumnName("new_status");

            builder.Property(osh => osh.Notes)
                .HasColumnName("notes");

            builder.Property(osh => osh.ChangedBy)
                .HasColumnName("changed_by");

            builder.Property(osh => osh.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(osh => osh.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(osh => osh.Order)
                .WithMany(o => o.OrderStatusHistories)
                .HasForeignKey(osh => osh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(osh => osh.ChangedByNavigation)
                .WithMany(u => u.OrderStatusHistories)
                .HasForeignKey(osh => osh.ChangedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(osh => osh.OrderId);
            builder.HasIndex(osh => osh.NewStatus);
            builder.HasIndex(osh => osh.CreatedAt);
            builder.HasIndex(osh => osh.ChangedBy);
        }
    }
}