namespace GreenSuppliers.Api.Models.Entities;

/// <summary>
/// Junction table linking SupplierProfile to UN SDGs (many-to-many).
/// </summary>
public class SupplierSdg
{
    public Guid SupplierProfileId { get; set; }
    public int SdgId { get; set; }

    // Navigation properties
    public SupplierProfile SupplierProfile { get; set; } = null!;
    public Sdg Sdg { get; set; } = null!;
}
