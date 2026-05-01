using System;
using System.ComponentModel.DataAnnotations;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.DTOs.coupon
{
    public class CouponUpdateDto
    {
        [StringLength(50)]
        public string? Code { get; set; }

        public string? Description { get; set; }
        public DiscountType? DiscountType { get; set; }

        [Range(typeof(decimal), "0", "999999999")]
        public decimal? DiscountValue { get; set; }

        public decimal? MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public bool? IsActive { get; set; }
    }
}