using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Models.Entities;

public class Organization
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? RegistrationNo { get; set; }

    [Column(TypeName = "char(2)")]
    public string CountryCode { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Province { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(254)]
    public string? Email { get; set; }

    [Column(TypeName = "nvarchar(20)")]
    public OrganizationType OrganizationType { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public SupplierProfile? SupplierProfile { get; set; }
}
