using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class Document
{
    public Guid Id { get; set; }

    public Guid SupplierProfileId { get; set; }

    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string BlobUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    [MaxLength(50)]
    public string DocumentType { get; set; } = string.Empty;

    public Guid? UploadedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public SupplierProfile SupplierProfile { get; set; } = null!;
    public User? UploadedByUser { get; set; }
}
