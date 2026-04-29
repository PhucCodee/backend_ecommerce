using System.Threading;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.payment;

namespace ECommerce.Application.Interfaces;

public interface IPaymentService
{
    Task<CreateMomoPaymentResponseDto> CreateMomoPaymentAsync(
        int userId,
        CreateMomoPaymentRequestDto request,
        CancellationToken ct
    );

    Task<MomoIpnResultDto> HandleMomoIpnAsync(MomoIpnDto request, CancellationToken ct);
}
