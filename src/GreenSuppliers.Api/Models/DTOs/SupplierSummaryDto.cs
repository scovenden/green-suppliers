using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Models.DTOs;

public class SupplierSummaryDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string? TradingName { get; set; }
    public string? ShortDescription { get; set; }
    public string? City { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public VerificationStatus VerificationStatus { get; set; }
    public EsgLevel EsgLevel { get; set; }
    public int EsgScore { get; set; }
    public string? LogoUrl { get; set; }
    public List<string> Industries { get; set; } = new();
    public bool IsVerified { get; set; }
}
