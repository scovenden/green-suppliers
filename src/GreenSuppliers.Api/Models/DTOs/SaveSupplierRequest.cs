using System.ComponentModel.DataAnnotations;

namespace GreenSuppliers.Api.Models.DTOs;

public class SaveSupplierRequest
{
    [Required]
    public Guid SupplierProfileId { get; set; }
}
