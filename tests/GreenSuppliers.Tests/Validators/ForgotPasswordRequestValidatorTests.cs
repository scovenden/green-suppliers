using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class ForgotPasswordRequestValidatorTests
{
    private readonly ForgotPasswordRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WithValidEmail()
    {
        var request = new ForgotPasswordRequest("user@example.com");
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenEmailIsEmpty()
    {
        var request = new ForgotPasswordRequest("");
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_WhenEmailIsInvalid()
    {
        var request = new ForgotPasswordRequest("not-an-email");
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_WhenEmailExceeds254Characters()
    {
        var longEmail = new string('a', 246) + "@test.com"; // 255 chars, exceeds 254 limit
        var request = new ForgotPasswordRequest(longEmail);
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
