using System;
using System.Collections.Generic;
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

namespace ECommerce.Infrastructure.Services.ZaloPay;

public sealed class ZaloPayPaymentGatewayClient(
    HttpClient httpClient,
    IOptions<ZaloPayOptions> options,
    ILogger<ZaloPayPaymentGatewayClient> logger
) : IPaymentGatewayClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly ZaloPayOptions _options = options.Value;
    private readonly ILogger<ZaloPayPaymentGatewayClient> _logger = logger;

    public async Task<PaymentGatewayResult> CreatePaymentAsync(
        PaymentCreateRequest request,
        CancellationToken ct
    )
    {
        var appTransId = request.OrderNumber;
        var appTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var amount = decimal.ToInt64(request.Amount).ToString();

        var embedData = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(_options.RedirectUrl))
            embedData["redirecturl"] = _options.RedirectUrl;

        var items = Array.Empty<object>();

        var param = new Dictionary<string, string>
        {
            ["app_id"] = _options.AppId,
            ["app_user"] = request.OrderId.ToString(),
            ["app_time"] = appTime,
            ["amount"] = amount,
            ["app_trans_id"] = appTransId,
            ["embed_data"] = JsonSerializer.Serialize(embedData),
            ["item"] = JsonSerializer.Serialize(items),
            ["description"] = request.Description,
            ["bank_code"] = _options.BankCode,
        };

        if (!string.IsNullOrWhiteSpace(_options.CallbackUrl))
            param["callback_url"] = _options.CallbackUrl;

        var rawData =
            _options.AppId
            + "|"
            + param["app_trans_id"]
            + "|"
            + param["app_user"]
            + "|"
            + param["amount"]
            + "|"
            + param["app_time"]
            + "|"
            + param["embed_data"]
            + "|"
            + param["item"];

        param["mac"] = SignHmacSha256(_options.Key1, rawData);

        using var content = new FormUrlEncodedContent(param);
        using var response = await _httpClient.PostAsync(_options.CreateOrderUrl, content, ct);
        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "ZaloPay create payment failed. Status={StatusCode}, Body={Body}",
                (int)response.StatusCode,
                responseBody
            );
            return new PaymentGatewayResult(
                false,
                appTransId,
                null,
                null,
                responseBody,
                response.StatusCode.ToString(),
                "ZaloPay HTTP error",
                null
            );
        }

        var dto = JsonSerializer.Deserialize<ZaloPayCreateResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        if (dto is null)
        {
            return new PaymentGatewayResult(
                false,
                appTransId,
                null,
                null,
                responseBody,
                "deserialize_error",
                "Invalid ZaloPay response",
                null
            );
        }

        var isSuccess = dto.ReturnCode == 1;

        return new PaymentGatewayResult(
            isSuccess,
            appTransId,
            dto.OrderUrl,
            null,
            responseBody,
            dto.ReturnCode.ToString(),
            dto.ReturnMessage ?? dto.SubReturnMessage,
            dto.ZpTransToken
        );
    }

    private static string SignHmacSha256(string secretKey, string rawData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var dataBytes = Encoding.UTF8.GetBytes(rawData);
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private sealed class ZaloPayCreateResponse
    {
        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string? ReturnMessage { get; set; }

        [JsonPropertyName("sub_return_message")]
        public string? SubReturnMessage { get; set; }

        [JsonPropertyName("order_url")]
        public string? OrderUrl { get; set; }

        [JsonPropertyName("zp_trans_token")]
        public string? ZpTransToken { get; set; }
    }
}
