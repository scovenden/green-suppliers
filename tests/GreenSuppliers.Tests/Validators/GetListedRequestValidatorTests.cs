using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class GetListedRequestValidatorTests
{
    private readonly GetListedRequestValidator _sut = new();

    private static GetListedRequest CreateValidRequest() => new()
    {
        CompanyName = "Green Energy Co",
        ContactName = "Alice Green",
        ContactEmail = "alice@greenenergy.co.za",
        Country = "ZA",
        Description = "We provide solar panel installations across South Africa."
    };

    [Fact]
    public void Should_Pass_WithValidRequest()
    {
        var result = _sut.TestValidate(CreateValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenCompanyNameIsEmpty()
    {
        var request = CreateValidRequest();
        request.CompanyName = "";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }

    [Fact]
    public void Should_Fail_WhenContactNameIsEmpty()
    {
        var request = CreateValidRequest();
        request.ContactName = "";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactName);
    }

    [Fact]
    public void Should_Fail_WhenContactEmailIsEmpty()
    {
        var request = CreateValidRequest();
        request.ContactEmail = "";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail);
    }

    [Fact]
    public void Should_Fail_WhenContactEmailIsInvalid()
    {
        var request = CreateValidRequest();
        request.ContactEmail = "not-an-email";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ContactEmail);
    }

    [Fact]
    public void Should_Fail_WhenCountryIsEmpty()
    {
        var request = CreateValidRequest();
        request.Country = "";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public void Should_Fail_WhenCountryIsNot2Characters()
    {
        var request = CreateValidRequest();
        request.Country = "ZAF";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Country);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionIsEmpty()
    {
        var request = CreateValidRequest();
        request.Description = "";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_WhenDescriptionExceeds500Characters()
    {
        var request = CreateValidRequest();
        request.Description = new string('A', 501);
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
