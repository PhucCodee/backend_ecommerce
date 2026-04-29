using System.Threading;
using System.Threading.Tasks;

namespace ECommerce.Domain.Interfaces;

public interface IPaymentGatewayClient
{
    Task<PaymentGatewayResult> CreatePaymentAsync(
        PaymentCreateRequest request,
        CancellationToken ct
    );
}

public record PaymentCreateRequest(
    int OrderId,
    string OrderNumber,
    decimal Amount,
    string Description
);

public record PaymentGatewayResult(
    bool IsSuccess,
    string? ProviderRequestId,
    string? PaymentUrl,
    string? QrCodeUrl,
    string? RawResponse,
    string? ErrorCode,
    string? ErrorMessage,
    string? ZpTransToken
);
