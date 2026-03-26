using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _sut = new();

    [Fact]
    public void should_pass_with_valid_email_and_password()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "AnyPassword1");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void should_fail_when_email_is_empty()
    {
        // Arrange
        var request = new LoginRequest("", "Password1");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void should_fail_when_email_is_invalid_format()
    {
        // Arrange
        var request = new LoginRequest("not-an-email", "Password1");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void should_fail_when_password_is_empty()
    {
        // Arrange
        var request = new LoginRequest("user@example.com", "");

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
