using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSuppliers.Api.Models.Entities;

public class Country
{
    [Column(TypeName = "char(2)")]
    public string Code { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Region { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
