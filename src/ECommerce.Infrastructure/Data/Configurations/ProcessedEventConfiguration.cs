using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class ProcessedEventConfiguration : IEntityTypeConfiguration<ProcessedEvent>
    {
        public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
        {
            builder.ToTable("processed_events");

            // Primary key
            builder.HasKey(pe => pe.EventId);
            builder.Property(pe => pe.EventId).HasColumnName("event_id");

            builder.Property(pe => pe.EventType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("event_type");

            builder.Property(pe => pe.ProcessedAt)
                .HasColumnName("processed_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(pe => pe.ProcessedBy)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("processed_by");

            // Indexes
            builder.HasIndex(pe => pe.EventType);
            builder.HasIndex(pe => pe.ProcessedAt);
        }
    }
}