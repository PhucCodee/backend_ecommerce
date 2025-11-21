using System;
using System.Collections.Generic;

namespace ECommerce.Domain.Entities;

public partial class OrderFulfillment
{
    public int FulfillmentId { get; set; }

    public int OrderId { get; set; }

    public required string TrackingNumber { get; set; }

    public required string Carrier { get; set; }

    public string? ShippingLabelUrl { get; set; }

    public DateTime? ShippedAt { get; set; }

    public DateOnly? EstimatedDeliveryDate { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public string? DeliveryProofUrl { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public required virtual Order Order { get; set; }
}
