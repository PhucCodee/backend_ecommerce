namespace ECommerce.Infrastructure.Services.ZaloPay;

public sealed class ZaloPayOptions
{
    public string AppId { get; set; } = string.Empty;
    public string Key1 { get; set; } = string.Empty;
    public string Key2 { get; set; } = string.Empty;
    public string CreateOrderUrl { get; set; } = "https://sb-openapi.zalopay.vn/v2/create";
    public string CallbackUrl { get; set; } = string.Empty;
    public string BankCode { get; set; } = "zalopayapp";
    public string RedirectUrl { get; set; } = string.Empty;
}
