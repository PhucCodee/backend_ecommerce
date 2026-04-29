using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Infrastructure.Services.Momo;

public sealed class MomoPaymentGatewayClient(
    HttpClient httpClient,
    IOptions<MomoOptions> options,
    ILogger<MomoPaymentGatewayClient> logger
) : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly MomoOptions _options = options.Value;
    private readonly ILogger<MomoPaymentGatewayClient> _logger = logger;

    public async Task<PaymentGatewayResult> CreatePaymentAsync(
        PaymentCreateRequest request,
        CancellationToken ct
    )
    {
        var requestId = Guid.NewGuid().ToString("N");
        var orderId = request.OrderNumber;
        var amount = decimal.ToInt64(request.Amount).ToString();
        const string extraData = "";

        var rawHash =
            "accessKey="
            + _options.AccessKey
            + "&amount="
            + amount
            + "&extraData="
            + extraData
            + "&ipnUrl="
            + _options.IpnUrl
            + "&orderId="
            + orderId
            + "&orderInfo="
            + request.Description
            + "&partnerCode="
            + _options.PartnerCode
            + "&redirectUrl="
            + _options.RedirectUrl
            + "&requestId="
            + requestId
            + "&requestType="
            + _options.RequestType;

        var signature = SignSha256(rawHash, _options.SecretKey);

        var payload = new MomoCreatePaymentRequest
        {
            PartnerCode = _options.PartnerCode,
            AccessKey = _options.AccessKey,
            RequestId = requestId,
            Amount = amount,
            OrderId = orderId,
            OrderInfo = request.Description,
            RedirectUrl = _options.RedirectUrl,
            IpnUrl = _options.IpnUrl,
            ExtraData = extraData,
            RequestType = _options.RequestType,
            Signature = signature,
            Lang = "en",
        };
        var json = JsonSerializer.Serialize(
            payload,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(
            _options.BaseUrl.TrimEnd('/') + "/v2/gateway/api/create",
            content,
            ct
        );
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "MoMo create payment failed. Status={StatusCode}, Body={Body}",
                (int)response.StatusCode,
                responseBody
            );
            return new PaymentGatewayResult(
                false,
                requestId,
                null,
                null,
                responseBody,
                response.StatusCode.ToString(),
                "MoMo HTTP error"
            );
        }

        var dto = JsonSerializer.Deserialize<MomoCreatePaymentResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        if (dto is null)
        {
            return new PaymentGatewayResult(
                false,
                requestId,
                null,
                null,
                responseBody,
                "deserialize_error",
                "Invalid MoMo response"
            );
        }

        var success = dto.ResultCode == 0;

        return new PaymentGatewayResult(
            success,
            requestId,
            dto.PayUrl,
            dto.QrCodeUrl,
            responseBody,
            dto.ResultCode.ToString(),
            dto.Message
        );
    }

    private static string SignSha256(string rawData, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(rawData);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private sealed class MomoCreatePaymentRequest
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
        public string ExtraData { get; set; } = string.Empty;
        public string RequestType { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string Lang { get; set; } = "en";
    }

    private sealed class MomoCreatePaymentResponse
    {
        public string? PartnerCode { get; set; }
        public string? RequestId { get; set; }
        public string? OrderId { get; set; }
        public long Amount { get; set; }
        public long ResponseTime { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("resultCode")]
        public int ResultCode { get; set; }

        [JsonPropertyName("payUrl")]
        public string? PayUrl { get; set; }

        [JsonPropertyName("qrCodeUrl")]
        public string? QrCodeUrl { get; set; }
        public string? Deeplink { get; set; }
    }
}
