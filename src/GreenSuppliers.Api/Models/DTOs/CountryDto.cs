namespace GreenSuppliers.Api.Models.DTOs;

public class CountryDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Region { get; set; }
    public bool IsActive { get; set; }
    public int SupplierCount { get; set; }
}
