using FluentAssertions;
using FluentValidation.TestHelper;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Validators;

namespace GreenSuppliers.Tests.Validators;

/// <summary>
/// Tests for UpdateMyProfileValidator — validates supplier self-service profile updates.
/// Covers: max lengths, email format, year bounds, percentage ranges, and valid inputs.
/// </summary>
public class UpdateMyProfileValidatorTests
{
    private readonly UpdateMyProfileValidator _sut = new();

    private static UpdateMyProfileRequest CreateValidRequest() => new()
    {
        TradingName = "Eco Solutions",
        Description = "A sustainable company.",
        ShortDescription = "Short desc",
        YearFounded = 2015,
        EmployeeCount = "50-100",
        BbbeeLevel = "Level 2",
        City = "Cape Town",
        Province = "Western Cape",
        Website = "https://ecosolutions.co.za",
        Phone = "+27211234567",
        Email = "info@ecosolutions.co.za",
        RenewableEnergyPercent = 50,
        WasteRecyclingPercent = 40,
        CarbonReporting = true,
        WaterManagement = false,
        SustainablePackaging = true
    };

    // =========================================================================
    // Valid input — should pass
    // =========================================================================

    [Fact]
    public void should_pass_with_all_valid_fields()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void should_pass_with_all_optional_fields_null()
    {
        // Arrange — all fields are optional in UpdateMyProfileRequest
        var request = new UpdateMyProfileRequest();

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void should_pass_when_email_is_null()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = null;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void should_pass_when_year_founded_is_null()
    {
        // Arrange
        var request = CreateValidRequest();
        request.YearFounded = null;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.YearFounded);
    }

    [Fact]
    public void should_pass_when_renewable_energy_is_null()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RenewableEnergyPercent = null;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RenewableEnergyPercent);
    }

    // =========================================================================
    // TradingName
    // =========================================================================

    [Fact]
    public void should_fail_when_trading_name_exceeds_200_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TradingName = new string('A', 201);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TradingName)
            .WithErrorMessage("Trading name must not exceed 200 characters.");
    }

    [Fact]
    public void should_pass_when_trading_name_is_exactly_200_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TradingName = new string('A', 200);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TradingName);
    }

    // =========================================================================
    // Description
    // =========================================================================

    [Fact]
    public void should_fail_when_description_exceeds_4000_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = new string('A', 4001);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 4000 characters.");
    }

    [Fact]
    public void should_pass_when_description_is_exactly_4000_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = new string('A', 4000);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // =========================================================================
    // ShortDescription
    // =========================================================================

    [Fact]
    public void should_fail_when_short_description_exceeds_500_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ShortDescription = new string('A', 501);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShortDescription)
            .WithErrorMessage("Short description must not exceed 500 characters.");
    }

    // =========================================================================
    // Email
    // =========================================================================

    [Fact]
    public void should_fail_when_email_is_invalid_format()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = "not-an-email";

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email must be a valid email address.");
    }

    [Fact]
    public void should_pass_when_email_is_empty_string()
    {
        // Arrange — empty string is treated as "not provided" (When condition)
        var request = CreateValidRequest();
        request.Email = "";

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void should_pass_when_email_is_valid()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Email = "supplier@example.co.za";

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    // =========================================================================
    // YearFounded
    // =========================================================================

    [Fact]
    public void should_fail_when_year_founded_is_before_1800()
    {
        // Arrange
        var request = CreateValidRequest();
        request.YearFounded = 1799;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.YearFounded);
    }

    [Fact]
    public void should_fail_when_year_founded_is_in_future()
    {
        // Arrange
        var request = CreateValidRequest();
        request.YearFounded = DateTime.UtcNow.Year + 1;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.YearFounded);
    }

    [Fact]
    public void should_pass_when_year_founded_is_1800()
    {
        // Arrange
        var request = CreateValidRequest();
        request.YearFounded = 1800;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.YearFounded);
    }

    [Fact]
    public void should_pass_when_year_founded_is_current_year()
    {
        // Arrange
        var request = CreateValidRequest();
        request.YearFounded = DateTime.UtcNow.Year;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.YearFounded);
    }

    // =========================================================================
    // EmployeeCount, BbbeeLevel, City, Province, Website, Phone
    // =========================================================================

    [Fact]
    public void should_fail_when_employee_count_exceeds_30_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.EmployeeCount = new string('X', 31);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmployeeCount)
            .WithErrorMessage("Employee count must not exceed 30 characters.");
    }

    [Fact]
    public void should_fail_when_bbbee_level_exceeds_20_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.BbbeeLevel = new string('X', 21);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BbbeeLevel)
            .WithErrorMessage("B-BBEE level must not exceed 20 characters.");
    }

    [Fact]
    public void should_fail_when_city_exceeds_100_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.City = new string('X', 101);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.City)
            .WithErrorMessage("City must not exceed 100 characters.");
    }

    [Fact]
    public void should_fail_when_province_exceeds_100_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Province = new string('X', 101);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Province)
            .WithErrorMessage("Province must not exceed 100 characters.");
    }

    [Fact]
    public void should_fail_when_website_exceeds_500_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Website = new string('X', 501);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Website)
            .WithErrorMessage("Website must not exceed 500 characters.");
    }

    [Fact]
    public void should_fail_when_phone_exceeds_30_characters()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Phone = new string('9', 31);

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("Phone must not exceed 30 characters.");
    }

    // =========================================================================
    // Percentage fields (RenewableEnergy, WasteRecycling)
    // =========================================================================

    [Fact]
    public void should_fail_when_renewable_energy_percent_exceeds_100()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RenewableEnergyPercent = 101;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RenewableEnergyPercent)
            .WithErrorMessage("Renewable energy percent must be between 0 and 100.");
    }

    [Fact]
    public void should_fail_when_renewable_energy_percent_is_negative()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RenewableEnergyPercent = -1;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RenewableEnergyPercent)
            .WithErrorMessage("Renewable energy percent must be between 0 and 100.");
    }

    [Fact]
    public void should_pass_when_renewable_energy_percent_is_0()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RenewableEnergyPercent = 0;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RenewableEnergyPercent);
    }

    [Fact]
    public void should_pass_when_renewable_energy_percent_is_100()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RenewableEnergyPercent = 100;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RenewableEnergyPercent);
    }

    [Fact]
    public void should_fail_when_waste_recycling_percent_exceeds_100()
    {
        // Arrange
        var request = CreateValidRequest();
        request.WasteRecyclingPercent = 101;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WasteRecyclingPercent)
            .WithErrorMessage("Waste recycling percent must be between 0 and 100.");
    }

    [Fact]
    public void should_fail_when_waste_recycling_percent_is_negative()
    {
        // Arrange
        var request = CreateValidRequest();
        request.WasteRecyclingPercent = -5;

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.WasteRecyclingPercent)
            .WithErrorMessage("Waste recycling percent must be between 0 and 100.");
    }

    // =========================================================================
    // Multiple validation errors
    // =========================================================================

    [Fact]
    public void should_return_multiple_errors_when_multiple_fields_invalid()
    {
        // Arrange
        var request = new UpdateMyProfileRequest
        {
            TradingName = new string('A', 201),
            Email = "not-valid",
            YearFounded = 1700,
            RenewableEnergyPercent = 150
        };

        // Act
        var result = _sut.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TradingName);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.YearFounded);
        result.ShouldHaveValidationErrorFor(x => x.RenewableEnergyPercent);
    }
}
