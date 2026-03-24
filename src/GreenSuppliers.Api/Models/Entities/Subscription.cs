using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.Entities;

public class Subscription
{
    public Guid Id { get; set; }

    public Guid OrganizationId { get; set; }
    public Guid PlanId { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "active";

    [MaxLength(10)]
    public string BillingCycle { get; set; } = "monthly";

    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(200)]
    public string? ExternalId { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Organization Organization { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
