using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Models.Entities;

public class SupplierCertification
{
    public Guid Id { get; set; }

    public Guid SupplierProfileId { get; set; }
    public Guid CertificationTypeId { get; set; }

    [MaxLength(100)]
    public string? CertificateNumber { get; set; }

    public DateOnly? IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }

    public Guid? DocumentId { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public CertificationStatus Status { get; set; } = CertificationStatus.Pending;

    public Guid? VerifiedByUserId { get; set; }
    public DateTime? VerifiedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public SupplierProfile SupplierProfile { get; set; } = null!;
    public CertificationType CertificationType { get; set; } = null!;
    public Document? Document { get; set; }
    public User? VerifiedByUser { get; set; }
}
