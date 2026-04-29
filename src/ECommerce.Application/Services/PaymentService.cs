using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Application.DTOs.payment;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Events;
using ECommerce.Domain.Interfaces;
using ECommerce.Domain.Repositories;
using ECommerce.Infrastructure.Services.Momo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Services;

public class PaymentService(
    IOrderRepository orderRepository,
    IOrderPaymentRepository orderPaymentRepository,
    IUnitOfWork unitOfWork,
    IPaymentGatewayClient paymentGatewayClient,
    IEventPublisher eventPublisher,
    IOptions<MomoOptions> momoOptions,
    ILogger<PaymentService> logger
) : IPaymentService
{
    private const string GatewayName = "momo";
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IOrderPaymentRepository _orderPaymentRepository = orderPaymentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPaymentGatewayClient _paymentGatewayClient = paymentGatewayClient;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly MomoOptions _momoOptions = momoOptions.Value;
    private readonly ILogger<PaymentService> _logger = logger;

    public async Task<CreateMomoPaymentResponseDto> CreateMomoPaymentAsync(
        int userId,
        CreateMomoPaymentRequestDto request,
        CancellationToken ct
    )
    {
        if (request.OrderId <= 0)
            throw new BadRequestException("OrderId is required");

        var order =
            await _orderRepository.GetByIdAsync(request.OrderId)
            ?? throw new NotFoundException("Order not found");

        if (order.UserId != userId)
            throw new ForbiddenException("Not allowed to pay this order");

        if (await _orderPaymentRepository.HasCompletedPaymentAsync(order.OrderId))
            throw new BadRequestException("Order already paid");

        var payment = await _orderPaymentRepository.GetLatestByOrderIdAndGatewayAsync(
            order.OrderId,
            GatewayName
        );

        if (payment is null)
        {
            payment = new OrderPayment
            {
                OrderId = order.OrderId,
                Method = PaymentMethod.e_wallet,
                Status = PaymentStatus.pending,
                Amount = order.TotalAmount,
                PaymentGateway = GatewayName,
                RetryCount = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Order = order,
            };

            await _orderPaymentRepository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        var gatewayResult = await _paymentGatewayClient.CreatePaymentAsync(
            new PaymentCreateRequest(
                OrderId: order.OrderId,
                OrderNumber: order.OrderNumber,
                Amount: order.TotalAmount,
                Description: request.Description ?? "Payment for order " + order.OrderNumber
            ),
            ct
        );

        if (!gatewayResult.IsSuccess)
        {
            payment.Status = PaymentStatus.failed;
            payment.FailureReason = gatewayResult.ErrorMessage ?? "MoMo create payment failed";
            payment.TransactionId = gatewayResult.ProviderRequestId;
            payment.GatewayResponse = JsonSerializer.Serialize(
                new { raw = gatewayResult.RawResponse }
            );
            payment.UpdatedAt = DateTime.UtcNow;

            await _eventPublisher.PublishAsync(
                new PaymentFailedEvent
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    SourceEventId = Guid.Empty,
                    Reason = payment.FailureReason,
                    ErrorCode = gatewayResult.ErrorCode,
                }
            );

            await _unitOfWork.SaveChangesAsync();
            throw new BadRequestException(payment.FailureReason);
        }

        payment.Status = PaymentStatus.pending;
        payment.TransactionId = gatewayResult.ProviderRequestId;
        payment.GatewayResponse = JsonSerializer.Serialize(
            new
            {
                paymentUrl = gatewayResult.PaymentUrl,
                qrCodeUrl = gatewayResult.QrCodeUrl,
                raw = gatewayResult.RawResponse,
            }
        );
        payment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new CreateMomoPaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            RequestId = gatewayResult.ProviderRequestId,
            PaymentUrl = gatewayResult.PaymentUrl,
            QrCodeUrl = gatewayResult.QrCodeUrl,
            Status = payment.Status.ToString(),
        };
    }

    public async Task<MomoIpnResultDto> HandleMomoIpnAsync(MomoIpnDto request, CancellationToken ct)
    {
        if (!IsValidSignature(request))
        {
            _logger.LogWarning("MoMo IPN signature invalid for orderId {OrderId}", request.OrderId);
            return new MomoIpnResultDto { ResultCode = 0, Message = "Ignored" };
        }

        if (string.IsNullOrWhiteSpace(request.OrderId))
        {
            return new MomoIpnResultDto { ResultCode = 0, Message = "OrderNotFoundIgnored" };
        }

        var orders = await _orderRepository.FindAsync(o => o.OrderNumber == request.OrderId);
        var order = orders.FirstOrDefault();

        if (order is null)
        {
            _logger.LogWarning("MoMo IPN order not found. orderId={OrderId}", request.OrderId);
            return new MomoIpnResultDto { ResultCode = 0, Message = "OrderNotFoundIgnored" };
        }

        var payment = await _orderPaymentRepository.GetLatestByOrderIdAndGatewayAsync(
            order.OrderId,
            GatewayName
        );

        if (payment is null)
        {
            _logger.LogWarning(
                "MoMo IPN payment record not found for order {OrderId}",
                order.OrderId
            );
            return new MomoIpnResultDto { ResultCode = 0, Message = "PaymentNotFoundIgnored" };
        }

        if (payment.Status == PaymentStatus.completed || payment.Status == PaymentStatus.failed)
        {
            return new MomoIpnResultDto { ResultCode = 0, Message = "AlreadyHandled" };
        }

        payment.TransactionId = request.TransId;
        payment.GatewayResponse = JsonSerializer.Serialize(request);
        payment.UpdatedAt = DateTime.UtcNow;

        if (request.ResultCode == 0)
        {
            payment.Status = PaymentStatus.completed;
            payment.PaidAt = DateTime.UtcNow;

            await _eventPublisher.PublishAsync(
                new PaymentSucceededEvent
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    SourceEventId = Guid.Empty,
                    TransactionId = request.TransId ?? request.RequestId ?? "momo",
                    Amount = order.TotalAmount,
                }
            );
        }
        else
        {
            payment.Status = PaymentStatus.failed;
            payment.FailureReason = request.Message ?? "MoMo payment failed";

            await _eventPublisher.PublishAsync(
                new PaymentFailedEvent
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    SourceEventId = Guid.Empty,
                    Reason = payment.FailureReason,
                    ErrorCode = request.ResultCode.ToString(),
                }
            );
        }

        await _unitOfWork.SaveChangesAsync();
        return new MomoIpnResultDto { ResultCode = 0, Message = "Confirm Success" };
    }

    private CreateMomoPaymentResponseDto MapExisting(OrderPayment payment)
    {
        string? paymentUrl = null;
        string? qrCodeUrl = null;

        if (!string.IsNullOrWhiteSpace(payment.GatewayResponse))
        {
            try
            {
                using var doc = JsonDocument.Parse(payment.GatewayResponse);
                if (doc.RootElement.TryGetProperty("paymentUrl", out var p))
                    paymentUrl = p.GetString();

                if (doc.RootElement.TryGetProperty("qrCodeUrl", out var q))
                    qrCodeUrl = q.GetString();
            }
            catch
            {
                // Ignore parse errors and return minimal response
            }
        }

        return new CreateMomoPaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            RequestId = payment.TransactionId,
            PaymentUrl = paymentUrl,
            QrCodeUrl = qrCodeUrl,
            Status = payment.Status.ToString(),
        };
    }

    private bool IsValidSignature(MomoIpnDto req)
    {
        var rawHash =
            "accessKey="
            + _momoOptions.AccessKey
            + "&amount="
            + req.Amount
            + "&extraData="
            + (req.ExtraData ?? string.Empty)
            + "&message="
            + (req.Message ?? string.Empty)
            + "&orderId="
            + (req.OrderId ?? string.Empty)
            + "&orderInfo="
            + (req.OrderInfo ?? string.Empty)
            + "&orderType="
            + (req.OrderType ?? string.Empty)
            + "&partnerCode="
            + (req.PartnerCode ?? string.Empty)
            + "&payType="
            + (req.PayType ?? string.Empty)
            + "&requestId="
            + (req.RequestId ?? string.Empty)
            + "&responseTime="
            + req.ResponseTime
            + "&resultCode="
            + req.ResultCode
            + "&transId="
            + (req.TransId ?? string.Empty);

        var expected = SignSha256(rawHash, _momoOptions.SecretKey);
        return string.Equals(expected, req.Signature, StringComparison.OrdinalIgnoreCase);
    }

    private static string SignSha256(string rawData, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(rawData);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
