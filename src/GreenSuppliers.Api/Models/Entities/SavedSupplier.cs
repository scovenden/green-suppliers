using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class SavedSupplier
{
    public Guid Id { get; set; }

    public Guid BuyerUserId { get; set; }

    public Guid SupplierProfileId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User BuyerUser { get; set; } = null!;
    public SupplierProfile SupplierProfile { get; set; } = null!;
}
