using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Application.Common.Responses;
using ECommerce.Application.DTOs.payment;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/payments/momo")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreateMomoPaymentRequestDto request,
        CancellationToken ct
    )
    {
        var userId = GetCurrentUserId();
        var result = await _paymentService.CreateMomoPaymentAsync(userId, request, ct);
        return Ok(
            ApiResponse<CreateMomoPaymentResponseDto>.Ok(result, "Payment created successfully")
        );
    }

    [AllowAnonymous]
    [HttpPost("ipn")]
    public async Task<IActionResult> MomoIpn([FromBody] MomoIpnDto request, CancellationToken ct)
    {
        var result = await _paymentService.HandleMomoIpnAsync(request, ct);
        return Ok(
            ApiResponse<object>.Ok(new { resultCode = result.ResultCode, message = result.Message })
        );
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedException("Invalid user token");
        return userId;
    }
}
