using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class EventLogConfiguration : IEntityTypeConfiguration<EventLog>
    {
        public void Configure(EntityTypeBuilder<EventLog> builder)
        {
            builder.ToTable("event_logs");

            // Primary key
            builder.HasKey(e => e.LogId);
            builder.Property(e => e.LogId).HasColumnName("log_id");

            // Properties
            builder.Property(e => e.EventId).HasColumnName("event_id");

            builder.Property(e => e.EventType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("event_type");

            builder.Property(e => e.AttemptNumber)
                .HasColumnName("attempt_number")
                .HasDefaultValue(1);

            builder.Property(e => e.Status)
                .HasColumnName("status")
                .HasColumnType("event_status_enum");

            builder.Property(e => e.WorkerName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("worker_name");

            builder.Property(e => e.OrderId)
                .HasColumnName("order_id");

            builder.Property(e => e.Payload)
                .HasColumnName("payload");

            builder.Property(e => e.ErrorMessage)
                .HasColumnName("error_message");

            builder.Property(e => e.ErrorCode)
                .HasMaxLength(50)
                .HasColumnName("error_code");

            builder.Property(e => e.StackTrace)
                .HasColumnName("stack_trace");

            builder.Property(e => e.StartedAt)
                .HasColumnName("started_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.CompletedAt)
                .HasColumnName("completed_at");

            builder.Property(e => e.ProcessingTimeMs)
                .HasColumnName("processing_time_ms");

            // Relationships
            builder.HasOne(e => e.Order)
                .WithMany(o => o.EventLogs)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            builder.HasIndex(e => e.EventId);
            builder.HasIndex(e => e.EventType);
            builder.HasIndex(e => e.Status);
            builder.HasIndex(e => e.StartedAt);
            builder.HasIndex(e => e.OrderId);
        }
    }
}