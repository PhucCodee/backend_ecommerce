using System.Collections.Generic;

namespace ECommerce.Application.DTOs.order;

public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public required string OrderNumber { get; set; }
    public required string Status { get; set; }
    public required string PaymentStatus { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public required string Currency { get; set; }
    public System.DateTime CreatedAt { get; set; }
}
