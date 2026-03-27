using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class UpdateSupplierValidatorTests
{
    private readonly UpdateSupplierValidator _sut = new();

    [Fact]
    public void Should_Pass_WithValidRequest()
    {
        var request = new UpdateSupplierRequest
        {
            CompanyName = "Updated Corp",
            CountryCode = "ZA"
        };

        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenCompanyNameIsEmpty()
    {
        var request = new UpdateSupplierRequest { CompanyName = "", CountryCode = "ZA" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }

    [Fact]
    public void Should_Fail_WhenCompanyNameExceeds200Characters()
    {
        var request = new UpdateSupplierRequest
        {
            CompanyName = new string('A', 201),
            CountryCode = "ZA"
        };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }

    [Fact]
    public void Should_Fail_WhenCountryCodeIsEmpty()
    {
        var request = new UpdateSupplierRequest { CompanyName = "Corp", CountryCode = "" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void Should_Fail_WhenCountryCodeIsNot2Characters()
    {
        var request = new UpdateSupplierRequest { CompanyName = "Corp", CountryCode = "Z" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void Should_Fail_WhenEmailIsInvalid()
    {
        var request = new UpdateSupplierRequest
        {
            CompanyName = "Corp",
            CountryCode = "ZA",
            Email = "invalid-email"
        };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Pass_WhenEmailIsNullOrWhitespace()
    {
        var request = new UpdateSupplierRequest
        {
            CompanyName = "Corp",
            CountryCode = "ZA",
            Email = null
        };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
