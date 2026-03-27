using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

/// <summary>
/// Records a single page view of a supplier profile for analytics.
/// </summary>
public class ProfileView
{
    public Guid Id { get; set; }

    public Guid SupplierProfileId { get; set; }

    [MaxLength(45)]
    public string? ViewerIp { get; set; }

    public Guid? ViewerUserId { get; set; }

    [MaxLength(2000)]
    public string? Referrer { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public SupplierProfile SupplierProfile { get; set; } = null!;
}
