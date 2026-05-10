using System;
using System.Collections.Generic;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

public partial class UserAddress
{
    public int AddressId { get; set; }

    public int UserId { get; set; }

    public AddressType Type { get; set; }

    public bool IsDefaultShipping { get; set; } = false;

    public bool IsDefaultBilling { get; set; } = false;

    public required string Label { get; set; } = string.Empty;

    public required string RecipientName { get; set; } = string.Empty;

    public required string Phone { get; set; } = string.Empty;

    public required string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    public required string City { get; set; } = string.Empty;

    public required string StateProvince { get; set; } = string.Empty;

    public required string PostalCode { get; set; } = string.Empty;

    public required string Country { get; set; } = string.Empty;

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual required User User { get; set; }

    public static UserAddress CreateDefault(
        User user,
        AddressType type,
        string label,
        string recipientName,
        string phone,
        string addressLine1,
        string city,
        string stateProvince,
        string postalCode,
        string country,
        string? addressLine2 = null,
        bool isDefaultShipping = false,
        bool isDefaultBilling = false
    )
    {
        return new UserAddress
        {
            User = user,
            UserId = user.UserId,
            Type = type,
            Label = label,
            RecipientName = recipientName,
            Phone = phone,
            AddressLine1 = addressLine1,
            AddressLine2 = addressLine2,
            City = city,
            StateProvince = stateProvince,
            PostalCode = postalCode,
            Country = country,
            IsDefaultShipping = isDefaultShipping,
            IsDefaultBilling = isDefaultBilling,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}
