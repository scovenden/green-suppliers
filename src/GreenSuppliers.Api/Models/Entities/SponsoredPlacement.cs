using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class SponsoredPlacement
{
    public Guid Id { get; set; }

    public Guid SupplierProfileId { get; set; }

    [MaxLength(30)]
    public string PlacementType { get; set; } = string.Empty;

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int ImpressionsCount { get; set; }
    public int ClicksCount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public SupplierProfile SupplierProfile { get; set; } = null!;
}
