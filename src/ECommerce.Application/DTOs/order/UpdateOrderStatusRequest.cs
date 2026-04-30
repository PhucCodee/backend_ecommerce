using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.order;

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
