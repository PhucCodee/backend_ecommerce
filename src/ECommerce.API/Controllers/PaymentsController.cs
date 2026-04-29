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
[Route("api/payments/zalopay")]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;

    [Authorize]
    [HttpPost("create")]
    public async Task<IActionResult> CreatePayment(
        [FromBody] CreateZaloPayPaymentRequestDto request,
        CancellationToken ct
    )
    {
        var userId = GetCurrentUserId();
        var result = await _paymentService.CreateZaloPayPaymentAsync(userId, request, ct);
        return Ok(
            ApiResponse<CreateZaloPayPaymentResponseDto>.Ok(result, "Payment created successfully")
        );
    }

    [AllowAnonymous]
    [HttpPost("callback")]
    public async Task<IActionResult> ZaloPayCallback(
        [FromBody] ZaloPayCallbackDto request,
        CancellationToken ct
    )
    {
        var result = await _paymentService.HandleZaloPayCallbackAsync(request, ct);
        return Ok(new { return_code = result.ReturnCode, return_message = result.ReturnMessage });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out int userId))
            throw new UnauthorizedException("Invalid user token");
        return userId;
    }
}
