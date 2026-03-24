using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class ServiceTag
{
    public Guid Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<SupplierServiceTag> SupplierServiceTags { get; set; } = new List<SupplierServiceTag>();
}
