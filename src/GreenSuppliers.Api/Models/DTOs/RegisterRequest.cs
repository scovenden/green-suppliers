namespace GreenSuppliers.Api.Models.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? CompanyName,
    string CountryCode,
    string AccountType // "supplier" or "buyer"
);
