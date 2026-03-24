using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Models.Entities;

public class Lead
{
    public Guid Id { get; set; }

    public Guid? SupplierProfileId { get; set; }
    public Guid? BuyerOrganizationId { get; set; }
    public Guid? BuyerUserId { get; set; }

    [MaxLength(150)]
    public string ContactName { get; set; } = string.Empty;

    [MaxLength(254)]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? CompanyName { get; set; }

    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(20)")]
    public LeadStatus Status { get; set; } = LeadStatus.New;

    [MaxLength(20)]
    public string LeadType { get; set; } = "inquiry";

    [MaxLength(45)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public SupplierProfile? SupplierProfile { get; set; }
    public Organization? BuyerOrganization { get; set; }
    public User? BuyerUser { get; set; }
}
