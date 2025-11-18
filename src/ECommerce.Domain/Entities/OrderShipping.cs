using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class OrderShipping
{
    public int ShippingId { get; set; }

    public int OrderId { get; set; }

    public string RecipientName { get; set; }

    public string Phone { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string StateProvince { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    public ShippingMethod Method { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Order Order { get; set; }
}
