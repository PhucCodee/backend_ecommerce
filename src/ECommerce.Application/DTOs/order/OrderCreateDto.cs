using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.order;

public class CreateOrderRequest
{
    // Existing saved address path
    public int? ShippingAddressId { get; set; }

    // New address path (for users with no saved addresses or one-time delivery)
    public NewShippingAddressRequest? NewShippingAddress { get; set; }

    // If true, persist NewShippingAddress into user_addresses
    public bool SaveNewShippingAddress { get; set; } = true;

    public int? BillingAddressId { get; set; }

    public string? CouponCode { get; set; }

    public string? CustomerNotes { get; set; }
}

public class NewShippingAddressRequest
{
    public AddressType Type { get; set; } = AddressType.house;

    public string Label { get; set; } = "Shipping";

    [Required]
    public string RecipientName { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string StateProvince { get; set; } = string.Empty;

    [Required]
    public string PostalCode { get; set; } = string.Empty;

    [Required]
    public string Country { get; set; } = string.Empty;
}