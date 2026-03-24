namespace GreenSuppliers.Api.Models.DTOs;

public class IndustryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int SupplierCount { get; set; }
}
