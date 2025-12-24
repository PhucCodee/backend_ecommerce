using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.cart
{
    public class UpdateCartItemDto
    {
        [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999")]
        public int Quantity { get; set; }
    }
}