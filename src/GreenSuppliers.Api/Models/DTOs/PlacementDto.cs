namespace GreenSuppliers.Api.Models.DTOs;

public class PlacementDto
{
    public Guid Id { get; set; }
    public Guid SupplierProfileId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierSlug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string PlacementType { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int ImpressionsCount { get; set; }
    public int ClicksCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreatePlacementRequest
{
    public Guid SupplierProfileId { get; set; }
    public string PlacementType { get; set; } = string.Empty;
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}

public class UpdatePlacementRequest
{
    public string? PlacementType { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool? IsActive { get; set; }
}
