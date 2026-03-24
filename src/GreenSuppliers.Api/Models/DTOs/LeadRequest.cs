using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.DTOs;

public class LeadRequest
{
    [Required]
    public Guid SupplierProfileId { get; set; }

    [Required, MaxLength(150)]
    public string ContactName { get; set; } = string.Empty;

    [Required, MaxLength(254), EmailAddress]
    public string ContactEmail { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? ContactPhone { get; set; }

    [MaxLength(200)]
    public string? CompanyName { get; set; }

    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
}
