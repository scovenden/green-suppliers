using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace GreenSuppliers.Api.Services;

public class PayFastService
{
    private readonly PayFastSettings _settings;
    private readonly ILogger<PayFastService> _logger;

    // PayFast sandbox server IPs that are allowed to send ITN callbacks.
    // In production, replace with live IPs from PayFast documentation.
    private static readonly string[] ValidPayFastHosts = new[]
    {
        "www.payfast.co.za",
        "sandbox.payfast.co.za",
        "w1w.payfast.co.za",
        "w2w.payfast.co.za"
    };

    public PayFastService(IOptions<PayFastSettings> settings, ILogger<PayFastService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Generates a PayFast checkout URL with MD5 signature for a one-time payment.
    /// </summary>
    public string GenerateCheckoutUrl(
        Guid paymentId,
        decimal amount,
        string itemName,
        string buyerEmail,
        string? buyerFirstName = null,
        string? buyerLastName = null)
    {
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", _settings.MerchantId),
            new("merchant_key", _settings.MerchantKey),
            new("return_url", _settings.ReturnUrl),
            new("cancel_url", _settings.CancelUrl),
            new("notify_url", _settings.NotifyUrl),
        };

        if (!string.IsNullOrEmpty(buyerFirstName))
            parameters.Add(new("name_first", buyerFirstName));
        if (!string.IsNullOrEmpty(buyerLastName))
            parameters.Add(new("name_last", buyerLastName));
        if (!string.IsNullOrEmpty(buyerEmail))
            parameters.Add(new("email_address", buyerEmail));

        parameters.Add(new("m_payment_id", paymentId.ToString()));
        parameters.Add(new("amount", amount.ToString("F2", CultureInfo.InvariantCulture)));
        parameters.Add(new("item_name", itemName));

        var signature = GenerateSignature(parameters);
        parameters.Add(new("signature", signature));

        var queryString = string.Join("&", parameters.Select(p =>
            $"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value)}"));

        var baseUrl = _settings.UseSandbox
            ? "https://sandbox.payfast.co.za/eng/process"
            : "https://www.payfast.co.za/eng/process";

        return $"{baseUrl}?{queryString}";
    }

    /// <summary>
    /// Generates the MD5 signature for a set of PayFast parameters.
    /// PayFast requires parameters to be in a specific order, URL-encoded, joined with &amp;,
    /// optionally appended with the passphrase, then MD5-hashed.
    /// </summary>
    public string GenerateSignature(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var paramString = string.Join("&", parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{WebUtility.UrlEncode(p.Key)}={WebUtility.UrlEncode(p.Value.Trim())}"));

        if (!string.IsNullOrEmpty(_settings.Passphrase))
        {
            paramString += $"&passphrase={WebUtility.UrlEncode(_settings.Passphrase.Trim())}";
        }

        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(paramString));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Validates an ITN (Instant Transaction Notification) callback from PayFast.
    /// Checks the MD5 signature against the received form data.
    /// </summary>
    public bool ValidateItnSignature(Dictionary<string, string> formData)
    {
        if (!formData.TryGetValue("signature", out var receivedSignature))
        {
            _logger.LogWarning("PayFast ITN: missing signature field");
            return false;
        }

        // Build parameter string from all fields except 'signature', in received order
        var parameters = formData
            .Where(kv => kv.Key != "signature" && !string.IsNullOrEmpty(kv.Value))
            .Select(kv => new KeyValuePair<string, string>(kv.Key, kv.Value));

        var expectedSignature = GenerateSignature(parameters);

        var isValid = string.Equals(receivedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase);

        if (!isValid)
        {
            _logger.LogWarning("PayFast ITN: signature mismatch. Received={Received}, Expected={Expected}",
                receivedSignature, expectedSignature);
        }

        return isValid;
    }

    /// <summary>
    /// Validates the PayFast server IP is from a known PayFast source.
    /// In production, verify against PayFast's published IP list.
    /// </summary>
    public bool ValidateSourceIp(string? remoteIp)
    {
        if (string.IsNullOrEmpty(remoteIp))
            return false;

        // In sandbox/development, accept all IPs
        if (_settings.UseSandbox)
            return true;

        try
        {
            foreach (var host in ValidPayFastHosts)
            {
                var hostAddresses = Dns.GetHostAddresses(host);
                if (hostAddresses.Any(ip => ip.ToString() == remoteIp))
                    return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayFast ITN: failed to resolve PayFast host IPs");
        }

        _logger.LogWarning("PayFast ITN: source IP {RemoteIp} is not a known PayFast server", remoteIp);
        return false;
    }
}
