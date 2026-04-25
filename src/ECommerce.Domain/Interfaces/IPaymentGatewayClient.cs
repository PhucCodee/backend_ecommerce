using System.Threading;
using System.Threading.Tasks;

public interface IPaymentGatewayClient
{
    Task<PaymentGatewayResult> ChargeAsync(PaymentChargeRequest request, CancellationToken ct);
}

public record PaymentChargeRequest(
    int OrderId,
    decimal Amount,
    string Method,
    string Currency,
    string Description);

public record PaymentGatewayResult(
    bool IsSuccess,
    string? TransactionId,
    string? GatewayResponse,
    string? ErrorCode,
    string? ErrorMessage);