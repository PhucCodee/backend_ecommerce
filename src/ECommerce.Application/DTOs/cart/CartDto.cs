using System;
using System.Collections.Generic;

namespace ECommerce.Application.DTOs.cart
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int? UserId { get; set; }
        public string? SessionId { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public int TotalItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<CartItemDto> Items { get; set; } = [];
    }
}