namespace GreenSuppliers.Api.Models.DTOs;

public class AddCertificationRequest
{
    public Guid CertificationTypeId { get; set; }
    public string? CertificateNumber { get; set; }
    public DateOnly? IssuedAt { get; set; }
    public DateOnly? ExpiresAt { get; set; }
    public Guid? DocumentId { get; set; }
}
