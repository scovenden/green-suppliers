using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSuppliers.Api.Models.Entities;

public class Payment
{
    public Guid Id { get; set; }

    public Guid SubscriptionId { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "char(3)")]
    public string Currency { get; set; } = "ZAR";

    [MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Provider { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ExternalId { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Subscription Subscription { get; set; } = null!;
}
