namespace GreenSuppliers.Api.Services;

public class PayFastSettings
{
    public string MerchantId { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string Passphrase { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.payfast.co.za";
    public string ReturnUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public bool UseSandbox { get; set; } = true;
}
