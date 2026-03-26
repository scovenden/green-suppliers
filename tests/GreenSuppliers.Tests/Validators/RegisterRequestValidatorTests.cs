using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _sut = new();

    private static RegisterRequest CreateValidSupplierRequest() => new(
        Email: "valid@example.com",
        Password: "SecurePass1",
        FirstName: "Jane",
        LastName: "Green",
        CompanyName: "EcoCorp",
        CountryCode: "ZA",
        AccountType: "supplier"
    );

    private static RegisterRequest CreateValidBuyerRequest() => new(
        Email: "buyer@example.com",
        Password: "SecurePass1",
        FirstName: "John",
        LastName: "Buyer",
        CompanyName: null,
        CountryCode: "ZA",
        AccountType: "buyer"
    );

    // =========================================================================
    // Happy path
    // =========================================================================

    [Fact]
    public void should_pass_with_valid_supplier_request()
    {
        // Act
        var result = _sut.TestValidate(CreateValidSupplierRequest());

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void should_pass_with_valid_buyer_request_without_company_name()
    {
        // Act
        var result = _sut.TestValidate(CreateValidBuyerRequest());

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void should_pass_with_buyer_providing_optional_company_name()
    {
        // Arrange
        var request = new RegisterRequest(
            Email: "buyer@example.com",
            Password: "SecurePass1",
            FirstName: "John",
            LastName: "Buyer",
            CompanyName: "Optional Corp",
            CountryCode: "ZA",
            AccountType: "buyer"
        );

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    // =========================================================================
    // Email validation
    // =========================================================================

    [Fact]
    public void should_fail_when_email_is_empty()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { Email = "" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void should_fail_when_email_is_invalid_format()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { Email = "not-an-email" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void should_fail_when_email_exceeds_254_characters()
    {
        // Arrange — create an email that exceeds 254 chars
        var longLocal = new string('a', 245);
        var request = CreateValidSupplierRequest() with { Email = $"{longLocal}@example.com" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    // =========================================================================
    // Password validation
    // =========================================================================

    [Fact]
    public void should_fail_when_password_is_empty()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { Password = "" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void should_fail_when_password_is_too_short()
    {
        // Arrange — less than 8 characters
        var request = CreateValidSupplierRequest() with { Password = "Pass1" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters.");
    }

    [Fact]
    public void should_fail_when_password_has_no_uppercase()
    {
        // Arrange — all lowercase + digit
        var request = CreateValidSupplierRequest() with { Password = "securepass1" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void should_fail_when_password_has_no_lowercase()
    {
        // Arrange — all uppercase + digit
        var request = CreateValidSupplierRequest() with { Password = "SECUREPASS1" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one lowercase letter.");
    }

    [Fact]
    public void should_fail_when_password_has_no_digit()
    {
        // Arrange — letters only
        var request = CreateValidSupplierRequest() with { Password = "SecurePass" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public void should_pass_when_password_meets_all_requirements()
    {
        // Arrange — exactly 8 chars, has upper, lower, digit
        var request = CreateValidSupplierRequest() with { Password = "Abcdefg1" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    // =========================================================================
    // Name validation
    // =========================================================================

    [Fact]
    public void should_fail_when_first_name_is_empty()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { FirstName = "" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void should_fail_when_last_name_is_empty()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { LastName = "" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void should_fail_when_first_name_exceeds_100_characters()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { FirstName = new string('A', 101) };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void should_fail_when_last_name_exceeds_100_characters()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { LastName = new string('A', 101) };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    // =========================================================================
    // Account type validation
    // =========================================================================

    [Fact]
    public void should_fail_when_account_type_is_empty()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { AccountType = "" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountType);
    }

    [Fact]
    public void should_fail_when_account_type_is_invalid()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { AccountType = "admin" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccountType)
            .WithErrorMessage("Account type must be 'supplier' or 'buyer'.");
    }

    // =========================================================================
    // Company name validation
    // =========================================================================

    [Fact]
    public void should_fail_when_supplier_has_no_company_name()
    {
        // Arrange — supplier with null company name
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: "SecurePass1",
            FirstName: "Jane",
            LastName: "Green",
            CompanyName: null,
            CountryCode: "ZA",
            AccountType: "supplier"
        );

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyName)
            .WithErrorMessage("Company name is required for supplier accounts.");
    }

    [Fact]
    public void should_fail_when_supplier_has_empty_company_name()
    {
        // Arrange — supplier with empty string company name
        var request = new RegisterRequest(
            Email: "test@example.com",
            Password: "SecurePass1",
            FirstName: "Jane",
            LastName: "Green",
            CompanyName: "",
            CountryCode: "ZA",
            AccountType: "supplier"
        );

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }

    [Fact]
    public void should_fail_when_company_name_exceeds_200_characters()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { CompanyName = new string('A', 201) };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompanyName);
    }

    // =========================================================================
    // Country code validation
    // =========================================================================

    [Fact]
    public void should_fail_when_country_code_is_empty()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { CountryCode = "" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }

    [Fact]
    public void should_fail_when_country_code_is_not_2_characters()
    {
        // Arrange — 3 characters
        var request = CreateValidSupplierRequest() with { CountryCode = "ZAF" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CountryCode)
            .WithErrorMessage("Country code must be exactly 2 characters.");
    }

    [Fact]
    public void should_fail_when_country_code_is_1_character()
    {
        // Arrange
        var request = CreateValidSupplierRequest() with { CountryCode = "Z" };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CountryCode);
    }
}
