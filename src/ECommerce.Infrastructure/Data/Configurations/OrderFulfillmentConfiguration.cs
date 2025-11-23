using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ECommerce.Domain.Entities;

namespace ECommerce.Infrastructure.Data.Configurations
{
    public class OrderFulfillmentConfiguration : IEntityTypeConfiguration<OrderFulfillment>
    {
        public void Configure(EntityTypeBuilder<OrderFulfillment> builder)
        {
            builder.ToTable("order_fulfillment");

            // Primary key
            builder.HasKey(of => of.FulfillmentId);
            builder.Property(of => of.FulfillmentId).HasColumnName("fulfillment_id");

            // Properties
            builder.Property(of => of.OrderId).HasColumnName("order_id");

            builder.Property(of => of.TrackingNumber)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tracking_number");

            builder.Property(of => of.Carrier)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("carrier");

            builder.Property(of => of.ShippingLabelUrl)
                .HasMaxLength(500)
                .HasColumnName("shipping_label_url");

            builder.Property(of => of.ShippedAt)
                .HasColumnName("shipped_at");

            builder.Property(of => of.EstimatedDeliveryDate)
                .HasColumnName("estimated_delivery_date");

            builder.Property(of => of.DeliveredAt)
                .HasColumnName("delivered_at");

            builder.Property(of => of.DeliveryProofUrl)
                .HasMaxLength(500)
                .HasColumnName("delivery_proof_url");

            builder.Property(of => of.Notes)
                .HasColumnName("notes");

            builder.Property(of => of.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(of => of.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Relationships
            builder.HasOne(of => of.Order)
                .WithOne(o => o.OrderFulfillment)
                .HasForeignKey<OrderFulfillment>(of => of.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(of => of.OrderId).IsUnique();
            builder.HasIndex(of => of.TrackingNumber).IsUnique();
            builder.HasIndex(of => of.Carrier);
            builder.HasIndex(of => of.ShippedAt);
            builder.HasIndex(of => of.DeliveredAt);
        }
    }
}