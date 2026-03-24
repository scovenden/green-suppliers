using FluentAssertions;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;

namespace GreenSuppliers.Tests.Services;

public class VerificationServiceTests
{
    private readonly VerificationService _sut = new();

    private static SupplierProfile CreateCompleteProfile() => new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = Guid.NewGuid(),
        TradingName = "Eco Corp",
        Description = "A sustainable supplier of green products.",
        CountryCode = "ZA",
        City = "Cape Town",
        VerificationStatus = VerificationStatus.Unverified
    };

    private static SupplierCertification CreateAcceptedCert(DateOnly? expiresAt = null) => new()
    {
        Id = Guid.NewGuid(),
        Status = CertificationStatus.Accepted,
        ExpiresAt = expiresAt ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
    };

    private static SupplierCertification CreatePendingCert() => new()
    {
        Id = Guid.NewGuid(),
        Status = CertificationStatus.Pending,
        ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
    };

    private static SupplierCertification CreateRejectedCert() => new()
    {
        Id = Guid.NewGuid(),
        Status = CertificationStatus.Rejected,
        ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
    };

    private static SupplierCertification CreateExpiredCert() => new()
    {
        Id = Guid.NewGuid(),
        Status = CertificationStatus.Accepted,
        ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1))
    };

    [Fact]
    public void Evaluate_NoCerts_ReturnsUnverified()
    {
        // Arrange
        var profile = CreateCompleteProfile();
        var certs = new List<SupplierCertification>();

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Unverified);
    }

    [Fact]
    public void Evaluate_PendingCert_ReturnsPending()
    {
        // Arrange
        var profile = CreateCompleteProfile();
        var certs = new List<SupplierCertification> { CreatePendingCert() };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Pending);
    }

    [Fact]
    public void Evaluate_AcceptedCertAndCompleteProfile_ReturnsVerified()
    {
        // Arrange
        var profile = CreateCompleteProfile();
        var certs = new List<SupplierCertification> { CreateAcceptedCert() };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Verified);
    }

    [Fact]
    public void Evaluate_AllCertsExpired_ReturnsUnverified()
    {
        // Arrange
        var profile = CreateCompleteProfile();
        var certs = new List<SupplierCertification>
        {
            CreateExpiredCert(),
            CreateExpiredCert()
        };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Unverified);
    }

    [Fact]
    public void Evaluate_FlaggedProfile_StaysFlagged()
    {
        // Arrange — profile is Flagged, even with valid certs it stays Flagged
        var profile = CreateCompleteProfile();
        profile.VerificationStatus = VerificationStatus.Flagged;
        var certs = new List<SupplierCertification> { CreateAcceptedCert() };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Flagged);
    }

    [Fact]
    public void Evaluate_IncompleteProfileWithAcceptedCert_ReturnsPending()
    {
        // Arrange — has accepted cert but TradingName is null => incomplete
        var profile = CreateCompleteProfile();
        profile.TradingName = null;
        var certs = new List<SupplierCertification> { CreateAcceptedCert() };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Pending);
    }

    [Fact]
    public void Evaluate_MixedCertStatuses_UsesHighestApplicable()
    {
        // Arrange — 1 Accepted + 1 Pending + 1 Rejected => Verified (accepted cert counts)
        var profile = CreateCompleteProfile();
        var certs = new List<SupplierCertification>
        {
            CreateAcceptedCert(),
            CreatePendingCert(),
            CreateRejectedCert()
        };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Verified);
    }

    [Fact]
    public void Evaluate_OnlyRejectedCerts_ReturnsUnverified()
    {
        // Arrange — all certs rejected
        var profile = CreateCompleteProfile();
        var certs = new List<SupplierCertification>
        {
            CreateRejectedCert(),
            CreateRejectedCert()
        };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Unverified);
    }

    [Fact]
    public void Evaluate_AcceptedCertWithNullExpiry_CountsAsValid()
    {
        // Arrange — cert with no expiry date should be considered valid
        var profile = CreateCompleteProfile();
        var certs = new List<SupplierCertification>
        {
            CreateAcceptedCert(expiresAt: null)
        };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Verified);
    }

    [Fact]
    public void Evaluate_IncompleteProfile_MissingDescription_ReturnsPending()
    {
        // Arrange — has accepted cert but Description is null
        var profile = CreateCompleteProfile();
        profile.Description = null;
        var certs = new List<SupplierCertification> { CreateAcceptedCert() };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Pending);
    }

    [Fact]
    public void Evaluate_IncompleteProfile_EmptyCountryCode_ReturnsPending()
    {
        // Arrange — has accepted cert but CountryCode is empty string
        var profile = CreateCompleteProfile();
        profile.CountryCode = "";
        var certs = new List<SupplierCertification> { CreateAcceptedCert() };

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Pending);
    }

    [Fact]
    public void Evaluate_IncompleteProfile_NoCerts_ReturnsUnverified()
    {
        // Arrange — incomplete profile AND no certs => Unverified (not Pending)
        var profile = CreateCompleteProfile();
        profile.TradingName = null;
        var certs = new List<SupplierCertification>();

        // Act
        var result = _sut.Evaluate(profile, certs);

        // Assert
        result.Should().Be(VerificationStatus.Unverified);
    }
}
