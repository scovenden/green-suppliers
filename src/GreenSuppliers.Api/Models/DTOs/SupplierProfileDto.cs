using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Models.DTOs;

public class SupplierProfileDto
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? TradingName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public int? YearFounded { get; set; }
    public string? EmployeeCount { get; set; }
    public string? BbbeeLevel { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? RenewableEnergyPercent { get; set; }
    public int? WasteRecyclingPercent { get; set; }
    public bool CarbonReporting { get; set; }
    public bool WaterManagement { get; set; }
    public bool SustainablePackaging { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public EsgLevel EsgLevel { get; set; }
    public int EsgScore { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? FlaggedReason { get; set; }
    public DateTime? LastScoredAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsVerified { get; set; }
    public List<SupplierIndustryDto> Industries { get; set; } = new();
    public List<SupplierServiceTagDto> ServiceTags { get; set; } = new();
    public List<SupplierCertificationDto> Certifications { get; set; } = new();
}

public class SupplierIndustryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class SupplierServiceTagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class SupplierCertificationDto
{
    public Guid Id { get; set; }
    public string CertificationTypeName { get; set; } = string.Empty;
    public string? CertificateNumber { get; set; }
    public DateOnly? IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
