namespace GreenSuppliers.Api.Models.DTOs;

public class UpdateMyProfileRequest
{
    public string? TradingName { get; set; }
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public int? YearFounded { get; set; }
    public string? EmployeeCount { get; set; }
    public string? BbbeeLevel { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Website { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int? RenewableEnergyPercent { get; set; }
    public int? WasteRecyclingPercent { get; set; }
    public bool CarbonReporting { get; set; }
    public bool WaterManagement { get; set; }
    public bool SustainablePackaging { get; set; }
    public List<Guid> IndustryIds { get; set; } = new();
    public List<Guid> ServiceTagIds { get; set; } = new();
}
