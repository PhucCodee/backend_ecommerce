using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserAddress
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public AddressType Type { get; set; }

    public bool IsDefaultShipping { get; set; }

    public bool IsDefaultBilling { get; set; }

    public string Label { get; set; }

    public string RecipientName { get; set; }

    public string Phone { get; set; }

    public string AddressLine1 { get; set; }

    public string AddressLine2 { get; set; }

    public string City { get; set; }

    public string StateProvince { get; set; }

    public string PostalCode { get; set; }

    public string Country { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User User { get; set; }
}
