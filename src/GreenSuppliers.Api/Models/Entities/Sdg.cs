using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

/// <summary>
/// One of the 17 UN Sustainable Development Goals.
/// Uses int PK (1-17) matching official SDG numbering.
/// </summary>
public class Sdg
{
    public int Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(7)]
    public string Color { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<SupplierSdg> SupplierSdgs { get; set; } = new List<SupplierSdg>();
}
