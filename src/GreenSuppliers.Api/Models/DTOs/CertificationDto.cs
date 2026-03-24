namespace GreenSuppliers.Api.Models.DTOs;

public class CertificationDto
{
    public Guid Id { get; set; }
    public Guid SupplierProfileId { get; set; }
    public string CertTypeName { get; set; } = string.Empty;
    public string CertTypeSlug { get; set; } = string.Empty;
    public string? CertificateNumber { get; set; }
    public DateOnly? IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
