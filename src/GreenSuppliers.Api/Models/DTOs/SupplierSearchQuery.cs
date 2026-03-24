namespace GreenSuppliers.Api.Models.DTOs;

public class SupplierSearchQuery
{
    public string? Q { get; set; }
    public string? CountryCode { get; set; }
    public string? IndustrySlug { get; set; }
    public string? EsgLevel { get; set; }
    public string? VerificationStatus { get; set; }
    public string? CertTypeSlug { get; set; }
    public string? Tags { get; set; }
    public string SortBy { get; set; } = "esgScore";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
