using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class VerifyEmailRequestValidatorTests
{
    private readonly VerifyEmailRequestValidator _sut = new();

    [Fact]
    public void Should_Pass_WithValidToken()
    {
        var request = new VerifyEmailRequest("ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890ABCDEF1234567890");
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenTokenIsEmpty()
    {
        var request = new VerifyEmailRequest("");
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Should_Fail_WhenTokenExceeds128Characters()
    {
        var longToken = new string('A', 129);
        var request = new VerifyEmailRequest(longToken);
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Token);
    }

    [Fact]
    public void Should_Pass_WhenTokenIsExactly128Characters()
    {
        var token = new string('A', 128);
        var request = new VerifyEmailRequest(token);
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Token);
    }
}
