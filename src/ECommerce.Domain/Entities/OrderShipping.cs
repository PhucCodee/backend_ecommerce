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

    public required virtual Order Order { get; set; }
}
