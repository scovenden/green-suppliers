using FluentAssertions;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;

namespace GreenSuppliers.Tests.Services;

public class EsgScoringServiceTests
{
    private readonly EsgScoringService _sut = new();

    private static SupplierProfile CreateCompleteProfile() => new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = Guid.NewGuid(),
        TradingName = "Eco Corp",
        Description = "A sustainable supplier of green products.",
        CountryCode = "ZA",
        City = "Cape Town"
    };

    private static SupplierCertification CreateValidCert() => new()
    {
        Id = Guid.NewGuid(),
        Status = CertificationStatus.Accepted,
        ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
    };

    [Fact]
    public void CalculateScore_IncompleteProfile_ReturnsNone()
    {
        // Arrange — missing TradingName and CountryCode
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            TradingName = null,
            Description = "Some description",
            CountryCode = "",
            City = null
        };

        // Act
        var result = _sut.CalculateScore(profile, new List<SupplierCertification>());

        // Assert
        result.Level.Should().Be(EsgLevel.None);
        result.NumericScore.Should().Be(0);
        result.Reasons.Should().Contain(r => r.Contains("TradingName"));
        result.Reasons.Should().Contain(r => r.Contains("CountryCode"));
        result.Reasons.Should().Contain(r => r.Contains("City"));
    }

    [Fact]
    public void CalculateScore_CompleteProfileNoCerts_ReturnsBronze()
    {
        // Arrange
        var profile = CreateCompleteProfile();

        // Act
        var result = _sut.CalculateScore(profile, new List<SupplierCertification>());

        // Assert
        result.Level.Should().Be(EsgLevel.Bronze);
        result.NumericScore.Should().Be(25);
        result.Reasons.Should().Contain(r => r.Contains("Profile complete"));
    }

    [Fact]
    public void CalculateScore_OneCert_Renewable20_ReturnsSilver()
    {
        // Arrange
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 20;

        var certs = new List<SupplierCertification> { CreateValidCert() };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert
        result.Level.Should().Be(EsgLevel.Silver);
        result.NumericScore.Should().Be(50);
    }

    [Fact]
    public void CalculateScore_TwoCerts_Renewable50_CarbonReporting_ReturnsGold()
    {
        // Arrange
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 50;
        profile.CarbonReporting = true;

        var certs = new List<SupplierCertification> { CreateValidCert(), CreateValidCert() };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert
        result.Level.Should().Be(EsgLevel.Gold);
        result.NumericScore.Should().Be(75);
    }

    [Fact]
    public void CalculateScore_ThreeCerts_Renewable70_Waste70_CarbonReporting_ReturnsPlatinum()
    {
        // Arrange
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 70;
        profile.WasteRecyclingPercent = 70;
        profile.CarbonReporting = true;

        var certs = new List<SupplierCertification>
        {
            CreateValidCert(), CreateValidCert(), CreateValidCert()
        };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert
        result.Level.Should().Be(EsgLevel.Platinum);
        result.NumericScore.Should().Be(100);
    }

    [Fact]
    public void CalculateScore_ExpiredCertsNotCounted()
    {
        // Arrange — 3 certs but 2 expired, only 1 valid => Silver at best
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 70;
        profile.WasteRecyclingPercent = 70;
        profile.CarbonReporting = true;

        var validCert = CreateValidCert();
        var expiredCert1 = new SupplierCertification
        {
            Id = Guid.NewGuid(),
            Status = CertificationStatus.Accepted,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
        };
        var expiredCert2 = new SupplierCertification
        {
            Id = Guid.NewGuid(),
            Status = CertificationStatus.Accepted,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30))
        };

        var certs = new List<SupplierCertification> { validCert, expiredCert1, expiredCert2 };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert — only 1 valid cert, so max Silver (needs >= 3 for Platinum)
        result.Level.Should().Be(EsgLevel.Silver);
        result.NumericScore.Should().Be(50);
    }

    [Fact]
    public void CalculateScore_RejectedCertsNotCounted()
    {
        // Arrange — 2 certs but 1 rejected, only 1 counts
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 50;
        profile.CarbonReporting = true;

        var validCert = CreateValidCert();
        var rejectedCert = new SupplierCertification
        {
            Id = Guid.NewGuid(),
            Status = CertificationStatus.Rejected,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };

        var certs = new List<SupplierCertification> { validCert, rejectedCert };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert — only 1 valid cert, cannot be Gold (needs >= 2)
        result.Level.Should().Be(EsgLevel.Silver);
        result.NumericScore.Should().Be(50);
    }

    [Fact]
    public void CalculateScore_BoundaryValues_Renewable19_NotSilver()
    {
        // Arrange — 1 valid cert but renewable at 19% (just under 20 threshold)
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 19;

        var certs = new List<SupplierCertification> { CreateValidCert() };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert — should be Bronze, not Silver
        result.Level.Should().Be(EsgLevel.Bronze);
        result.NumericScore.Should().Be(25);
    }

    [Fact]
    public void CalculateScore_BoundaryValues_Renewable20_IsSilver()
    {
        // Arrange — 1 valid cert and renewable at exactly 20%
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 20;

        var certs = new List<SupplierCertification> { CreateValidCert() };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert
        result.Level.Should().Be(EsgLevel.Silver);
        result.NumericScore.Should().Be(50);
    }

    [Fact]
    public void CalculateScore_ReturnsReasonsList()
    {
        // Arrange — Platinum profile to get all positive reasons
        var profile = CreateCompleteProfile();
        profile.RenewableEnergyPercent = 80;
        profile.WasteRecyclingPercent = 75;
        profile.CarbonReporting = true;

        var certs = new List<SupplierCertification>
        {
            CreateValidCert(), CreateValidCert(), CreateValidCert()
        };

        // Act
        var result = _sut.CalculateScore(profile, certs);

        // Assert
        result.Reasons.Should().NotBeEmpty();
        result.Reasons.Should().Contain(r => r.Contains("Profile complete"));
        result.Reasons.Should().Contain(r => r.Contains("3 valid certification"));
        result.Reasons.Should().Contain(r => r.Contains("Renewable energy"));
        result.Reasons.Should().Contain(r => r.Contains("Waste recycling"));
        result.Reasons.Should().Contain(r => r.Contains("Carbon reporting"));
    }
}
