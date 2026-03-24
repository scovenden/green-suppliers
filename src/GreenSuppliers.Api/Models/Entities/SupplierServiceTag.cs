namespace GreenSuppliers.Api.Models.Entities;

public class SupplierServiceTag
{
    public Guid SupplierProfileId { get; set; }
    public Guid ServiceTagId { get; set; }

    // Navigation properties
    public SupplierProfile SupplierProfile { get; set; } = null!;
    public ServiceTag ServiceTag { get; set; } = null!;
}
