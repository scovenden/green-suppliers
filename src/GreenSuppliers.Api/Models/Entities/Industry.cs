using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class Industry
{
    public Guid Id { get; set; }

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Industry? Parent { get; set; }
    public ICollection<Industry> Children { get; set; } = new List<Industry>();
    public ICollection<SupplierIndustry> SupplierIndustries { get; set; } = new List<SupplierIndustry>();
}
