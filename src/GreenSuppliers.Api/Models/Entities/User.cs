using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Models.Entities;

public class User
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }

    [MaxLength(254)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(30)")]
    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;
    public bool EmailVerified { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
}
