namespace GreenSuppliers.Api.Models.Entities;

public class SupplierIndustry
{
    public Guid SupplierProfileId { get; set; }
    public Guid IndustryId { get; set; }

    // Navigation properties
    public SupplierProfile SupplierProfile { get; set; } = null!;
    public Industry Industry { get; set; } = null!;
}
