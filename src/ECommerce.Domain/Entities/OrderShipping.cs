using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class OrderShipping
{
    public int ShippingId { get; set; }

    public int OrderId { get; set; }

    public required string RecipientName { get; set; }

    public required string Phone { get; set; }

    public required string AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public required string City { get; set; }

    public required string StateProvince { get; set; }

    public required string PostalCode { get; set; }

    public required string Country { get; set; }

    public ShippingMethod Method { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual required Order Order { get; set; }

    public static OrderShipping CreateDefault(
        Order order,
        string recipientName,
        string phone,
        string addressLine1,
        string city,
        string stateProvince,
        string postalCode,
        string country,
        ShippingMethod method = ShippingMethod.standard,
        string? addressLine2 = null
    )
    {
        return new OrderShipping
        {
            OrderId = order.OrderId,
            RecipientName = recipientName,
            Phone = phone,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            StateProvince = stateProvince,
            PostalCode = postalCode,
            Country = country,
            Method = method,
            CreatedAt = DateTime.UtcNow,
            Order = order,
        };
    }
}
