using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.address
{
    public class AddressUpdateDto
    {
        public AddressType? Type { get; set; }

        [StringLength(100)]
        public string? Label { get; set; }

        [StringLength(255)]
        public string? RecipientName { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(255)]
        public string? AddressLine1 { get; set; }

        [StringLength(255)]
        public string? AddressLine2 { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? StateProvince { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        public bool? IsDefaultShipping { get; set; }
        public bool? IsDefaultBilling { get; set; }
    }
}