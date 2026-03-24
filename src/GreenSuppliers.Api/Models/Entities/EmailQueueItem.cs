using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class EmailQueueItem
{
    public Guid Id { get; set; }

    [MaxLength(254)]
    public string ToEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    public string ToName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    public string BodyHtml { get; set; } = string.Empty;

    [MaxLength(50)]
    public string TemplateType { get; set; } = string.Empty;

    public string? TemplateData { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "pending";

    public int RetryCount { get; set; }
    public DateTime? SentAt { get; set; }

    [MaxLength(2000)]
    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}
