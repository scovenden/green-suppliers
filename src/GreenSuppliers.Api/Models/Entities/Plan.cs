using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GreenSuppliers.Api.Models.Entities;

public class Plan
{
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string DisplayName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceMonthly { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PriceYearly { get; set; }

    [Column(TypeName = "char(3)")]
    public string Currency { get; set; } = "ZAR";

    public int? MaxLeadsPerMonth { get; set; }
    public int? MaxDocuments { get; set; }
    public bool FeaturedListing { get; set; }
    public bool AnalyticsAccess { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
