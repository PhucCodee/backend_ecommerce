using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderPaymentConfiguration : IEntityTypeConfiguration<OrderPayment>
    {
        public void Configure(EntityTypeBuilder<OrderPayment> builder)
        {
            builder.ToTable("order_payments");

            // Primary key
            builder.HasKey(op => op.PaymentId);
            builder.Property(op => op.PaymentId).HasColumnName("payment_id");

            // Properties
            builder.Property(op => op.OrderId).HasColumnName("order_id");

            builder.Property(op => op.Method)
                .HasConversion<string>()
                .HasColumnName("method");

            builder.Property(op => op.Status)
                .HasConversion<string>()
                .HasColumnName("status")
                .HasDefaultValue(PaymentStatus.Pending);

            builder.Property(op => op.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasColumnName("amount");

            builder.Property(op => op.PaymentGateway)
                .HasMaxLength(100)
                .HasColumnName("payment_gateway");

            builder.Property(op => op.TransactionId)
                .HasMaxLength(255)
                .HasColumnName("transaction_id");

            builder.Property(op => op.GatewayResponse)
                .HasColumnName("gateway_response");

            builder.Property(op => op.PaidAt)
                .HasColumnName("paid_at");

            builder.Property(op => op.RefundedAt)
                .HasColumnName("refunded_at");

            builder.Property(op => op.FailureReason)
                .HasColumnName("failure_reason");

            builder.Property(op => op.RetryCount)
                .HasColumnName("retry_count")
                .HasDefaultValue(0);

            builder.Property(op => op.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(op => op.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(op => op.Order)
                .WithMany(o => o.OrderPayments)
                .HasForeignKey(op => op.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(op => op.OrderId);
            builder.HasIndex(op => op.TransactionId);
            builder.HasIndex(op => op.Status);
            builder.HasIndex(op => op.Method);
            builder.HasIndex(op => op.CreatedAt);
        }
    }
}