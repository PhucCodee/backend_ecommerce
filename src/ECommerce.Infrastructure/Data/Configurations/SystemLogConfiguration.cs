using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
    {
        public void Configure(EntityTypeBuilder<SystemLog> builder)
        {
            builder.ToTable("system_logs");

            // Primary key
            builder.HasKey(sl => sl.LogId);
            builder.Property(sl => sl.LogId).HasColumnName("log_id");

            // Properties
            builder.Property(sl => sl.LogLevel)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("log_level");

            builder.Property(sl => sl.LogType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("log_type");

            builder.Property(sl => sl.Source)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("source");

            builder.Property(sl => sl.Message)
                .IsRequired()
                .HasColumnName("message");

            builder.Property(sl => sl.Details)
                .HasColumnName("details");

            builder.Property(sl => sl.UserId)
                .HasColumnName("user_id");

            builder.Property(sl => sl.RequestId)
                .HasColumnName("request_id");

            builder.Property(sl => sl.IpAddress)
                .IsRequired()
                .HasMaxLength(45)
                .HasColumnName("ip_address");

            builder.Property(sl => sl.StackTrace)
                .HasColumnName("stack_trace");

            builder.Property(sl => sl.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(sl => sl.User)
                .WithMany(u => u.SystemLogs)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            builder.HasIndex(sl => sl.LogLevel);
            builder.HasIndex(sl => sl.LogType);
            builder.HasIndex(sl => sl.CreatedAt);
            builder.HasIndex(sl => sl.UserId);
        }
    }
}