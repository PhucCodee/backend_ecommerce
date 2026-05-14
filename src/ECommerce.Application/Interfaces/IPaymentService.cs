using System.Threading;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.payment;

namespace ECommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<CreateZaloPayPaymentResponseDto> CreateZaloPayPaymentAsync(
        int userId,
        CreateZaloPayPaymentRequestDto request,
        CancellationToken ct
    );
    Task<ZaloPayCallbackResultDto> HandleZaloPayCallbackAsync(
        ZaloPayCallbackDto request,
        CancellationToken ct
    );
    Task<ZaloPayCallbackResultDto> SimulateZaloPayCallbackAsync(
        string appTransId,
        decimal amount,
        CancellationToken ct
    );
}
