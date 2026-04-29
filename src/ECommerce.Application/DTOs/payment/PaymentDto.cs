using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.DTOs.payment;

public sealed class CreateMomoPaymentRequestDto
{
    [Required]
    public int OrderId { get; set; }

    public string? Description { get; set; }
}

public sealed class CreateMomoPaymentResponseDto
{
    public int PaymentId { get; set; }
    public string? RequestId { get; set; }
    public string? PaymentUrl { get; set; }
    public string? QrCodeUrl { get; set; }
    public string? Status { get; set; }
}

public sealed class MomoIpnDto
{
    public string? PartnerCode { get; set; }
    public string? OrderId { get; set; }
    public string? RequestId { get; set; }
    public string? Amount { get; set; }
    public string? OrderInfo { get; set; }
    public string? OrderType { get; set; }
    public string? TransId { get; set; }
    public int ResultCode { get; set; }
    public string? Message { get; set; }
    public string? PayType { get; set; }
    public long ResponseTime { get; set; }
    public string? ExtraData { get; set; }
    public string? Signature { get; set; }
}

public sealed class MomoIpnResultDto
{
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
}
