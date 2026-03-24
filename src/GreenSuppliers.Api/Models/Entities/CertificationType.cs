using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class CertificationType
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<SupplierCertification> SupplierCertifications { get; set; } = new List<SupplierCertification>();
}
