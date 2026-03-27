namespace GreenSuppliers.Api.Models.DTOs;

public class PlanDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal PriceMonthly { get; set; }
    public decimal PriceYearly { get; set; }
    public string Currency { get; set; } = "ZAR";
    public int? MaxLeadsPerMonth { get; set; }
    public int? MaxDocuments { get; set; }
    public bool FeaturedListing { get; set; }
    public bool AnalyticsAccess { get; set; }
    public bool PrioritySupport { get; set; }
    public int TrialDays { get; set; }
    public int SortOrder { get; set; }
}
