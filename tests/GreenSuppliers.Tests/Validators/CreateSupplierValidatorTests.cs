using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class CreateSupplierValidatorTests
{
    private readonly CreateSupplierValidator _sut = new();

    [Fact]
    public void Should_Pass_WithValidRequest()
    {
        var request = new CreateSupplierRequest
        {
            CompanyName = "Eco Solutions (Pty) Ltd",
            CountryCode = "ZA"
        };

        var result = _sut.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_WhenCompanyNameIsEmpty()
    {
        var request = new CreateSupplierRequest { CompanyName = "", CountryCode = "ZA" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }

    [Fact]
    public void Should_Fail_WhenCompanyNameExceeds200Characters()
    {
        var request = new CreateSupplierRequest
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
        var request = new CreateSupplierRequest { CompanyName = "Eco Corp", CountryCode = "" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void Should_Fail_WhenCountryCodeIsNot2Characters()
    {
        var request = new CreateSupplierRequest { CompanyName = "Eco Corp", CountryCode = "ZAF" };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void Should_Fail_WhenEmailIsInvalid()
    {
        var request = new CreateSupplierRequest
        {
            CompanyName = "Eco Corp",
            CountryCode = "ZA",
            Email = "not-an-email"
        };
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Pass_WhenEmailIsNullOrEmpty()
    {
        // Email is optional -- should not fail when null or empty
        var request = new CreateSupplierRequest
        {
            CompanyName = "Eco Corp",
            CountryCode = "ZA",
            Email = null
        };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);

        request.Email = "";
        result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Pass_WhenEmailIsValid()
    {
        var request = new CreateSupplierRequest
        {
            CompanyName = "Eco Corp",
            CountryCode = "ZA",
            Email = "info@ecosolutions.co.za"
        };
        var result = _sut.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }
}
