using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.payment;

public sealed class CreateZaloPayPaymentRequestDto
{
    [Required]
    public int OrderId { get; set; }
    public string? Description { get; set; }
}

public sealed class CreateZaloPayPaymentResponseDto
{
    public int PaymentId { get; set; }
    public string? AppTransId { get; set; }
    public string? OrderUrl { get; set; }
    public string? ZpTransToken { get; set; }
    public string? Status { get; set; }
}

public sealed class ZaloPayCallbackDto
{
    public string? Data { get; set; }
    public string? Mac { get; set; }
    public int Type { get; set; }
}

public sealed class ZaloPayCallbackResultDto
{
    public int ReturnCode { get; set; }
    public string ReturnMessage { get; set; } = "success";
}

public class SimulateZaloPayCallbackRequestDto
{
    public string AppTransId { get; set; } = "";
    public decimal Amount { get; set; }
}
