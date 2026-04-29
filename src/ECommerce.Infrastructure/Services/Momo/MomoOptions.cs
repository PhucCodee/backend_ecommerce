namespace ECommerce.Infrastructure.Services.Momo;

public sealed class MomoOptions
{
    public string BaseUrl { get; set; } = "https://test-payment.momo.vn";
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public string IpnUrl { get; set; } = string.Empty;
    public string RequestType { get; set; } = "captureWallet";
}
