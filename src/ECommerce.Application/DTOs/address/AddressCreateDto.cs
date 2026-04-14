using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.address
{
    public class AddressCreateDto
    {
        public AddressType Type { get; set; } = AddressType.house;

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = "Home";

        [Required]
        [StringLength(255)]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string AddressLine1 { get; set; } = string.Empty;

        [StringLength(255)]
        public string? AddressLine2 { get; set; }

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string StateProvince { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        public bool IsDefaultShipping { get; set; } = false;
        public bool IsDefaultBilling { get; set; } = false;
    }
}