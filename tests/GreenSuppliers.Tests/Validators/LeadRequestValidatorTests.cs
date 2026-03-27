using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class LeadRequestValidatorTests
{
    private readonly LeadRequestValidator _sut = new();

    private static LeadRequest CreateValidRequest() => new()
    {
        SupplierProfileId = Guid.NewGuid(),
        ContactName = "John Doe",
        ContactEmail = "john@example.com",
        Message = "I am interested in your sustainable packaging solutions."
    };

    [Fact]
    public void Should_Pass_WithValidRequest()
    {
        var result = _sut.TestValidate(CreateValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
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
    public void Should_Fail_WhenContactNameExceeds150Characters()
    {
        var request = CreateValidRequest();
        request.ContactName = new string('A', 151);
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
    public void Should_Fail_WhenMessageIsEmpty()
    {
        var request = CreateValidRequest();
        request.Message = "";
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Should_Fail_WhenMessageExceeds2000Characters()
    {
        var request = CreateValidRequest();
        request.Message = new string('A', 2001);
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Message);
    }

    [Fact]
    public void Should_Fail_WhenSupplierProfileIdIsEmpty()
    {
        var request = CreateValidRequest();
        request.SupplierProfileId = Guid.Empty;
        var result = _sut.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SupplierProfileId);
    }
}
