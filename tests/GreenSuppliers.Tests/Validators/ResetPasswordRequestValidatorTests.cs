using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class ResetPasswordRequestValidatorTests
{
    private readonly ResetPasswordRequestValidator _sut = new();

    private static ResetPasswordRequest CreateValidRequest() =>
        new("VALID_TOKEN_ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF12", "NewSecure1");

    // =========================================================================
    // Happy path
    // =========================================================================

    [Fact]
    public void should_pass_with_valid_token_and_password()
    {
        // Act
        var result = _sut.TestValidate(CreateValidRequest());

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // =========================================================================
    // Token validation
    // =========================================================================

    [Fact]
    public void should_fail_when_token_is_empty()
    {
        // Arrange
        var request = new ResetPasswordRequest("", "NewSecure1");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Token)
            .WithErrorMessage("Reset token is required.");
    }

    // =========================================================================
    // Password validation (same rules as registration)
    // =========================================================================

    [Fact]
    public void should_fail_when_password_is_empty()
    {
        // Arrange
        var request = new ResetPasswordRequest("VALID_TOKEN", "");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void should_fail_when_password_is_too_short()
    {
        // Arrange
        var request = new ResetPasswordRequest("VALID_TOKEN", "Pass1");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void should_fail_when_password_has_no_uppercase()
    {
        // Arrange
        var request = new ResetPasswordRequest("VALID_TOKEN", "securepass1");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void should_fail_when_password_has_no_lowercase()
    {
        // Arrange
        var request = new ResetPasswordRequest("VALID_TOKEN", "SECUREPASS1");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void should_fail_when_password_has_no_digit()
    {
        // Arrange
        var request = new ResetPasswordRequest("VALID_TOKEN", "SecurePass");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Password must contain at least one digit.");
    }
}
