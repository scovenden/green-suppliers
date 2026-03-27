namespace GreenSuppliers.Api.Models.DTOs;

public class SupplierSearchResult
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string TradingName { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? City { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string VerificationStatus { get; set; } = string.Empty;
    public string EsgLevel { get; set; } = string.Empty;
    public int EsgScore { get; set; }
    public string? LogoUrl { get; set; }
    public List<string> Industries { get; set; } = new();
    public List<int> SdgIds { get; set; } = new();
    public bool IsVerified { get; set; }
}
