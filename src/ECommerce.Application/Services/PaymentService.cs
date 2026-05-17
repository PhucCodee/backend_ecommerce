using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
using ECommerce.Infrastructure.Services.ZaloPay;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Application.Services;

public class PaymentService(
    IOrderRepository orderRepository,
    IOrderPaymentRepository orderPaymentRepository,
    IUnitOfWork unitOfWork,
    IPaymentGatewayClient paymentGatewayClient,
    IEventPublisher eventPublisher,
    IOptions<ZaloPayOptions> zaloPayOptions,
    IHttpClientFactory httpClientFactory,
    ILogger<PaymentService> logger
) : IPaymentService
{
    private const string GatewayName = "zalopay";
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IOrderPaymentRepository _orderPaymentRepository = orderPaymentRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPaymentGatewayClient _paymentGatewayClient = paymentGatewayClient;
    private readonly IEventPublisher _eventPublisher = eventPublisher;
    private readonly ZaloPayOptions _zaloPayOptions = zaloPayOptions.Value;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<PaymentService> _logger = logger;

    public async Task<CreateZaloPayPaymentResponseDto> CreateZaloPayPaymentAsync(
        int userId,
        CreateZaloPayPaymentRequestDto request,
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

        var appTransId = ResolveAppTransId(
            order.OrderNumber,
            payment.TransactionId,
            payment.PaymentId
        );

        if (!string.Equals(payment.TransactionId, appTransId, StringComparison.Ordinal))
        {
            payment.TransactionId = appTransId;
            payment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();
        }

        var gatewayResult = await _paymentGatewayClient.CreatePaymentAsync(
            new PaymentCreateRequest(
                OrderId: order.OrderId,
                OrderNumber: appTransId,
                Amount: order.TotalAmount,
                Description: request.Description ?? "Payment for order " + order.OrderId
            ),
            ct
        );

        if (!gatewayResult.IsSuccess)
        {
            payment.Status = PaymentStatus.failed;
            payment.FailureReason = gatewayResult.ErrorMessage ?? "ZaloPay create payment failed";
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
                    Amount = order.TotalAmount,
                }
            );

            await _unitOfWork.SaveChangesAsync();
            throw new BadRequestException(payment.FailureReason);
        }

        payment.Status = PaymentStatus.pending;
        payment.GatewayResponse = JsonSerializer.Serialize(
            new
            {
                orderUrl = gatewayResult.PaymentUrl,
                zpTransToken = gatewayResult.ZpTransToken,
                raw = gatewayResult.RawResponse,
            }
        );
        payment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();

        return new CreateZaloPayPaymentResponseDto
        {
            PaymentId = payment.PaymentId,
            AppTransId = appTransId,
            OrderUrl = gatewayResult.PaymentUrl,
            ZpTransToken = gatewayResult.ZpTransToken,
            Status = payment.Status.ToString(),
        };
    }

    public async Task<ZaloPayCallbackResultDto> HandleZaloPayCallbackAsync(
        ZaloPayCallbackDto request,
        CancellationToken ct
    )
    {
        if (request.Type != 1)
        {
            return new ZaloPayCallbackResultDto { ReturnCode = 1, ReturnMessage = "ignored" };
        }

        if (string.IsNullOrWhiteSpace(request.Data) || string.IsNullOrWhiteSpace(request.Mac))
        {
            return new ZaloPayCallbackResultDto
            {
                ReturnCode = -1,
                ReturnMessage = "invalid payload",
            };
        }

        var generatedMac = SignHmacSha256(_zaloPayOptions.Key2, request.Data);

        if (!string.Equals(generatedMac, request.Mac, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "ZaloPay callback MAC invalid. Please check Key2 and data integrity."
            );
            return new ZaloPayCallbackResultDto { ReturnCode = -1, ReturnMessage = "invalid mac" };
        }

        ZaloPayCallbackData? data;
        try
        {
            data = JsonSerializer.Deserialize<ZaloPayCallbackData>(
                request.Data,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (Exception)
        {
            return new ZaloPayCallbackResultDto { ReturnCode = 0, ReturnMessage = "invalid data" };
        }

        if (data is null || string.IsNullOrWhiteSpace(data.AppTransId))
        {
            return new ZaloPayCallbackResultDto
            {
                ReturnCode = 0,
                ReturnMessage = "missing app_trans_id",
            };
        }

        var payments = await _orderPaymentRepository.FindAsync(p =>
            p.PaymentGateway == GatewayName && p.TransactionId == data.AppTransId
        );
        var payment = payments.OrderByDescending(p => p.CreatedAt).FirstOrDefault();

        if (payment is null)
        {
            _logger.LogWarning(
                "ZaloPay callback payment not found. app_trans_id={AppTransId}",
                data.AppTransId
            );
            return new ZaloPayCallbackResultDto
            {
                ReturnCode = 0,
                ReturnMessage = "payment not found",
            };
        }

        var order = await _orderRepository.GetByIdAsync(payment.OrderId);
        if (order is null)
        {
            _logger.LogWarning(
                "ZaloPay callback order not found. orderId={OrderId}",
                payment.OrderId
            );
            return new ZaloPayCallbackResultDto
            {
                ReturnCode = 0,
                ReturnMessage = "order not found",
            };
        }

        if (payment.Status == PaymentStatus.completed || payment.Status == PaymentStatus.failed)
        {
            return new ZaloPayCallbackResultDto
            {
                ReturnCode = 1,
                ReturnMessage = "already handled",
            };
        }

        var expectedAmount = decimal.ToInt64(order.TotalAmount);
        if (data.Amount > 0 && data.Amount != expectedAmount)
        {
            payment.Status = PaymentStatus.failed;
            payment.FailureReason = "Amount mismatch";
            payment.GatewayResponse = JsonSerializer.Serialize(
                new
                {
                    data = request.Data,
                    mac = request.Mac,
                    type = request.Type,
                    zpTransId = data.ZpTransId,
                }
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
                    ErrorCode = "amount_mismatch",
                    Amount = order.TotalAmount,
                }
            );

            await _unitOfWork.SaveChangesAsync();
            return new ZaloPayCallbackResultDto
            {
                ReturnCode = 1,
                ReturnMessage = "amount mismatch",
            };
        }

        payment.GatewayResponse = JsonSerializer.Serialize(
            new
            {
                data = request.Data,
                mac = request.Mac,
                type = request.Type,
                zpTransId = data.ZpTransId,
            }
        );
        payment.UpdatedAt = DateTime.UtcNow;

        var isPaymentSuccess = data.ZpTransId.HasValue && data.ZpTransId.Value > 0;
        if (isPaymentSuccess)
        {
            payment.Status = PaymentStatus.completed;
            payment.PaidAt = DateTime.UtcNow;

            if (order.Status == OrderStatus.created)
            {
                order.Status = OrderStatus.confirmed;
                order.UpdatedAt = DateTime.UtcNow;
            }

            await _eventPublisher.PublishAsync(
                new PaymentSucceededEvent
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    SourceEventId = Guid.Empty,
                    TransactionId = data.ZpTransId?.ToString() ?? "",
                    Amount = order.TotalAmount,
                }
            );
        }
        else
        {
            payment.Status = PaymentStatus.failed;
            payment.FailureReason = "ZaloPay callback missing zp_trans_id";

            await _eventPublisher.PublishAsync(
                new PaymentFailedEvent
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    UserId = order.UserId,
                    SourceEventId = Guid.Empty,
                    Reason = payment.FailureReason,
                    ErrorCode = "missing_zp_trans_id",
                    Amount = order.TotalAmount,
                }
            );
        }

        await _unitOfWork.SaveChangesAsync();

        return new ZaloPayCallbackResultDto { ReturnCode = 1, ReturnMessage = "success" };
    }

    public async Task<ZaloPayCallbackResultDto> SimulateZaloPayCallbackAsync(
        string appTransId,
        decimal amount,
        CancellationToken ct
    )
    {
        var callbackData = new ZaloPayCallbackData
        {
            AppTransId = appTransId,
            ZpTransId = 260517000000563,
            ReturnCode = 1,
            ReturnMessage = "Success",
            Amount = (long)amount,
        };

        var dataJson = JsonSerializer.Serialize(
            callbackData,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
        );
        var mac = SignHmacSha256(_zaloPayOptions.Key2, dataJson);

        var callbackPayload = new ZaloPayCallbackDto
        {
            Data = dataJson,
            Mac = mac,
            Type = 1,
        };

        return await HandleZaloPayCallbackAsync(callbackPayload, ct);
    }

    private static string ResolveAppTransId(
        string orderNumber,
        string? existingTransId,
        int paymentId
    )
    {
        if (IsValidAppTransId(existingTransId))
            return existingTransId!;

        if (IsValidAppTransId(orderNumber))
            return orderNumber;

        // Append a random 6-digit number to ensure uniqueness across restarts
        var randomSuffix = new Random().Next(100000, 999999);
        return $"{DateTime.UtcNow:yyMMdd}_{paymentId}_{randomSuffix}";
    }

    private static bool IsValidAppTransId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var parts = value.Split('_');
        // Now expecting 3 parts: yyMMdd_paymentId_random
        if (parts.Length != 3 && parts.Length != 2)
            return false;

        if (
            !DateTime.TryParseExact(
                parts[0],
                "yyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _
            )
        )
            return false;

        // Check that the rest of the parts are digits
        for (int i = 1; i < parts.Length; i++)
        {
            if (!parts[i].All(char.IsDigit))
                return false;
        }

        return true;
    }

    private static string SignHmacSha256(string secretKey, string rawData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(rawData);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private sealed class ZaloPayCallbackData
    {
        [JsonPropertyName("app_trans_id")]
        public string? AppTransId { get; set; }

        [JsonPropertyName("zp_trans_id")]
        public long? ZpTransId { get; set; }

        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string? ReturnMessage { get; set; }

        [JsonPropertyName("amount")]
        public long Amount { get; set; }
    }
}
