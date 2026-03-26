using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.DTOs;

public class UpdateLeadStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
