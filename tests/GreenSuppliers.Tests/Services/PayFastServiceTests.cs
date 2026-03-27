using FluentAssertions;
using GreenSuppliers.Api.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GreenSuppliers.Tests.Services;

public class PayFastServiceTests
{
    private static PayFastService CreateService(PayFastSettings? settings = null)
    {
        settings ??= new PayFastSettings
        {
            MerchantId = "10000100",
            MerchantKey = "46f0cd694581a",
            Passphrase = "",
            BaseUrl = "https://sandbox.payfast.co.za",
            ReturnUrl = "http://localhost:3000/billing/success",
            CancelUrl = "http://localhost:3000/billing/cancel",
            NotifyUrl = "https://localhost:5001/api/v1/webhooks/payfast/itn",
            UseSandbox = true
        };
        var opts = Options.Create(settings);
        var logger = new Mock<ILogger<PayFastService>>();
        return new PayFastService(opts, logger.Object);
    }

    // =========================================================================
    // GenerateCheckoutUrl
    // =========================================================================

    [Fact]
    public void GenerateCheckoutUrl_ContainsMerchantId()
    {
        // Arrange
        var service = CreateService();
        var paymentId = Guid.NewGuid();

        // Act
        var url = service.GenerateCheckoutUrl(paymentId, 499m, "Pro Plan", "test@test.com");

        // Assert
        url.Should().Contain("merchant_id=10000100");
    }

    [Fact]
    public void GenerateCheckoutUrl_ContainsSandboxBaseUrl()
    {
        // Arrange
        var service = CreateService();

        // Act
        var url = service.GenerateCheckoutUrl(Guid.NewGuid(), 499m, "Pro Plan", "test@test.com");

        // Assert
        url.Should().StartWith("https://sandbox.payfast.co.za/eng/process?");
    }

    [Fact]
    public void GenerateCheckoutUrl_ContainsAmount()
    {
        // Arrange
        var service = CreateService();

        // Act
        var url = service.GenerateCheckoutUrl(Guid.NewGuid(), 999.50m, "Premium Plan", "test@test.com");

        // Assert
        url.Should().Contain("amount=999.50");
    }

    [Fact]
    public void GenerateCheckoutUrl_ContainsSignature()
    {
        // Arrange
        var service = CreateService();

        // Act
        var url = service.GenerateCheckoutUrl(Guid.NewGuid(), 499m, "Pro Plan", "test@test.com");

        // Assert
        url.Should().Contain("signature=");
    }

    [Fact]
    public void GenerateCheckoutUrl_ProductionUrl_WhenSandboxDisabled()
    {
        // Arrange
        var service = CreateService(new PayFastSettings
        {
            MerchantId = "10000100",
            MerchantKey = "46f0cd694581a",
            Passphrase = "",
            UseSandbox = false,
            ReturnUrl = "https://example.com/return",
            CancelUrl = "https://example.com/cancel",
            NotifyUrl = "https://example.com/notify"
        });

        // Act
        var url = service.GenerateCheckoutUrl(Guid.NewGuid(), 499m, "Pro Plan", "test@test.com");

        // Assert
        url.Should().StartWith("https://www.payfast.co.za/eng/process?");
    }

    // =========================================================================
    // GenerateSignature / ValidateItnSignature
    // =========================================================================

    [Fact]
    public void GenerateSignature_IsDeterministic()
    {
        // Arrange
        var service = CreateService();
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("merchant_id", "10000100"),
            new("amount", "499.00"),
            new("item_name", "Pro Plan")
        };

        // Act
        var sig1 = service.GenerateSignature(parameters);
        var sig2 = service.GenerateSignature(parameters);

        // Assert
        sig1.Should().Be(sig2);
        sig1.Should().HaveLength(32); // MD5 produces 32 hex chars
    }

    [Fact]
    public void ValidateItnSignature_ValidSignature_ReturnsTrue()
    {
        // Arrange
        var service = CreateService();
        var formData = new Dictionary<string, string>
        {
            { "m_payment_id", Guid.NewGuid().ToString() },
            { "payment_status", "COMPLETE" },
            { "amount_gross", "499.00" }
        };

        // Generate valid signature
        var signature = service.GenerateSignature(formData.Select(kv =>
            new KeyValuePair<string, string>(kv.Key, kv.Value)));
        formData["signature"] = signature;

        // Act
        var result = service.ValidateItnSignature(formData);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateItnSignature_InvalidSignature_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var formData = new Dictionary<string, string>
        {
            { "m_payment_id", Guid.NewGuid().ToString() },
            { "payment_status", "COMPLETE" },
            { "amount_gross", "499.00" },
            { "signature", "definitely_wrong_signature" }
        };

        // Act
        var result = service.ValidateItnSignature(formData);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateItnSignature_MissingSignature_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();
        var formData = new Dictionary<string, string>
        {
            { "m_payment_id", Guid.NewGuid().ToString() },
            { "payment_status", "COMPLETE" }
            // no signature field
        };

        // Act
        var result = service.ValidateItnSignature(formData);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateSignature_WithPassphrase_ProducesDifferentHash()
    {
        // Arrange
        var serviceWithout = CreateService(new PayFastSettings
        {
            MerchantId = "10000100",
            MerchantKey = "46f0cd694581a",
            Passphrase = "",
            UseSandbox = true,
            ReturnUrl = "",
            CancelUrl = "",
            NotifyUrl = ""
        });
        var serviceWith = CreateService(new PayFastSettings
        {
            MerchantId = "10000100",
            MerchantKey = "46f0cd694581a",
            Passphrase = "my_secret_passphrase",
            UseSandbox = true,
            ReturnUrl = "",
            CancelUrl = "",
            NotifyUrl = ""
        });
        var parameters = new List<KeyValuePair<string, string>>
        {
            new("amount", "499.00")
        };

        // Act
        var sigWithout = serviceWithout.GenerateSignature(parameters);
        var sigWith = serviceWith.GenerateSignature(parameters);

        // Assert
        sigWithout.Should().NotBe(sigWith);
    }

    // =========================================================================
    // ValidateSourceIp
    // =========================================================================

    [Fact]
    public void ValidateSourceIp_Sandbox_AcceptsAnyIp()
    {
        // Arrange
        var service = CreateService(); // UseSandbox = true by default

        // Act & Assert
        service.ValidateSourceIp("1.2.3.4").Should().BeTrue();
        service.ValidateSourceIp("192.168.0.1").Should().BeTrue();
    }

    [Fact]
    public void ValidateSourceIp_NullIp_ReturnsFalse()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        service.ValidateSourceIp(null).Should().BeFalse();
        service.ValidateSourceIp("").Should().BeFalse();
    }
}
