using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.DTOs;

public class GetListedRequest
{
    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string ContactName { get; set; } = string.Empty;

    [Required, MaxLength(254), EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    public List<Guid>? IndustryIds { get; set; }

    [Required, MaxLength(2)]
    public string Country { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? City { get; set; }

    [Required, MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Certifications { get; set; }
}
