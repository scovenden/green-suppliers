using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Models.Entities;

public class SupplierProfile
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    [MaxLength(250)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TradingName { get; set; }

    [MaxLength(4000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ShortDescription { get; set; }

    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    [MaxLength(1000)]
    public string? BannerUrl { get; set; }

    public int? YearFounded { get; set; }

    [MaxLength(30)]
    public string? EmployeeCount { get; set; }

    [MaxLength(20)]
    public string? BbbeeLevel { get; set; }

    [Column(TypeName = "char(2)")]
    public string CountryCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    public int? RenewableEnergyPercent { get; set; }
    public int? WasteRecyclingPercent { get; set; }
    public bool CarbonReporting { get; set; }
    public bool WaterManagement { get; set; }
    public bool SustainablePackaging { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.Unverified;

    [Column(TypeName = "nvarchar(20)")]
    public EsgLevel EsgLevel { get; set; } = EsgLevel.None;

    public int EsgScore { get; set; }

    public bool IsPublished { get; set; } = true;
    public DateTime? PublishedAt { get; set; }

    [MaxLength(500)]
    public string? FlaggedReason { get; set; }

    public DateTime? LastScoredAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public ICollection<SupplierIndustry> SupplierIndustries { get; set; } = new List<SupplierIndustry>();
    public ICollection<SupplierServiceTag> SupplierServiceTags { get; set; } = new List<SupplierServiceTag>();
    public ICollection<SupplierCertification> Certifications { get; set; } = new List<SupplierCertification>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();
}
