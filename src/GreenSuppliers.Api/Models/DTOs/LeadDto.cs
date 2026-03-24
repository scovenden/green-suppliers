namespace GreenSuppliers.Api.Models.DTOs;

public class LeadDto
{
    public Guid Id { get; set; }
    public Guid SupplierProfileId { get; set; }
    public Guid? BuyerOrganizationId { get; set; }
    public Guid? BuyerUserId { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? CompanyName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LeadType { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
