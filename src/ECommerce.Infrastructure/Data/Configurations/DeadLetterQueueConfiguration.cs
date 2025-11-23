using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class DeadLetterQueueConfiguration : IEntityTypeConfiguration<DeadLetterQueue>
    {
        public void Configure(EntityTypeBuilder<DeadLetterQueue> builder)
        {
            builder.ToTable("dead_letter_queue");

            builder.HasKey(d => d.DlqId);
            builder.Property(d => d.DlqId).HasColumnName("dlq_id");

            builder.Property(d => d.EventId).HasColumnName("event_id");
            builder.Property(d => d.EventType).HasColumnName("event_type");
            builder.Property(d => d.OriginalQueue).HasColumnName("original_queue");
            builder.Property(d => d.Payload).HasColumnName("payload");
            builder.Property(d => d.FinalErrorMessage).HasColumnName("final_error_message");
            builder.Property(d => d.TotalRetryAttempts).HasColumnName("total_retry_attempts");
            builder.Property(d => d.FirstFailedAt).HasColumnName("first_failed_at");
            builder.Property(d => d.MovedToDlqAt).HasColumnName("moved_to_dlq_at");
            builder.Property(d => d.ResolutionStatus).HasColumnName("resolution_status");
            builder.Property(d => d.ReprocessedAt).HasColumnName("reprocessed_at");
            builder.Property(d => d.ReprocessedBy).HasColumnName("reprocessed_by");
            builder.Property(d => d.ResolutionNotes).HasColumnName("resolution_notes");

            // Relationship
            builder.HasOne(d => d.ReprocessedByNavigation)
                .WithMany(u => u.DeadLetterQueues)
                .HasForeignKey(d => d.ReprocessedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(d => d.EventId);
            builder.HasIndex(d => d.EventType);
            builder.HasIndex(d => d.ResolutionStatus);
        }
    }
}