using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

/// <summary>
/// Tests for AddCertificationValidator — validates supplier certification submissions.
/// Covers: required CertificationTypeId, max length on CertificateNumber,
/// and date order (ExpiresAt must be after IssuedAt).
/// </summary>
public class AddCertificationValidatorTests
{
    private readonly AddCertificationValidator _sut = new();

    // =========================================================================
    // Valid input — should pass
    // =========================================================================

    [Fact]
    public void should_pass_with_all_valid_fields()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            CertificateNumber = "CERT-2026-001",
            IssuedAt = new DateOnly(2025, 1, 1),
            ExpiresAt = new DateOnly(2028, 1, 1),
            DocumentId = Guid.NewGuid()
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void should_pass_with_only_required_fields()
    {
        // Arrange — CertificationTypeId is the only required field
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid()
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void should_pass_when_certificate_number_is_null()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            CertificateNumber = null
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CertificateNumber);
    }

    [Fact]
    public void should_pass_when_dates_are_null()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            IssuedAt = null,
            ExpiresAt = null
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.IssuedAt);
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiresAt);
    }

    [Fact]
    public void should_pass_when_only_issued_at_provided()
    {
        // Arrange — only IssuedAt, no ExpiresAt (the When condition requires both)
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            IssuedAt = new DateOnly(2025, 1, 1),
            ExpiresAt = null
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiresAt);
    }

    [Fact]
    public void should_pass_when_only_expires_at_provided()
    {
        // Arrange — only ExpiresAt, no IssuedAt (the When condition requires both)
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            IssuedAt = null,
            ExpiresAt = new DateOnly(2028, 1, 1)
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiresAt);
    }

    // =========================================================================
    // CertificationTypeId — required
    // =========================================================================

    [Fact]
    public void should_fail_when_certification_type_id_is_empty()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.Empty
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CertificationTypeId)
            .WithErrorMessage("Certification type is required.");
    }

    // =========================================================================
    // CertificateNumber — max length
    // =========================================================================

    [Fact]
    public void should_fail_when_certificate_number_exceeds_100_characters()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            CertificateNumber = new string('C', 101)
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CertificateNumber)
            .WithErrorMessage("Certificate number must not exceed 100 characters.");
    }

    [Fact]
    public void should_pass_when_certificate_number_is_exactly_100_characters()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            CertificateNumber = new string('C', 100)
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CertificateNumber);
    }

    // =========================================================================
    // Date ordering — ExpiresAt must be after IssuedAt
    // =========================================================================

    [Fact]
    public void should_fail_when_expires_at_is_before_issued_at()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            IssuedAt = new DateOnly(2026, 6, 1),
            ExpiresAt = new DateOnly(2025, 1, 1)
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiresAt)
            .WithErrorMessage("Expiry date must be after the issue date.");
    }

    [Fact]
    public void should_fail_when_expires_at_equals_issued_at()
    {
        // Arrange — same date should fail (GreaterThan, not GreaterThanOrEqual)
        var sameDate = new DateOnly(2026, 6, 1);
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            IssuedAt = sameDate,
            ExpiresAt = sameDate
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiresAt)
            .WithErrorMessage("Expiry date must be after the issue date.");
    }

    [Fact]
    public void should_pass_when_expires_at_is_after_issued_at()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(),
            IssuedAt = new DateOnly(2025, 1, 1),
            ExpiresAt = new DateOnly(2025, 1, 2)
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiresAt);
    }

    // =========================================================================
    // Multiple validation errors
    // =========================================================================

    [Fact]
    public void should_return_multiple_errors_when_multiple_fields_invalid()
    {
        // Arrange
        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.Empty,
            CertificateNumber = new string('X', 101),
            IssuedAt = new DateOnly(2026, 6, 1),
            ExpiresAt = new DateOnly(2025, 1, 1)
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CertificationTypeId);
        result.ShouldHaveValidationErrorFor(x => x.CertificateNumber);
        result.ShouldHaveValidationErrorFor(x => x.ExpiresAt);
    }
}
