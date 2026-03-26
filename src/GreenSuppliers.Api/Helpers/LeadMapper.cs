using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;

namespace GreenSuppliers.Api.Helpers;

/// <summary>
/// Shared Lead -> LeadDto mapping used by SupplierMeService, LeadService, and BuyerService.
/// Extracted to eliminate three identical copies of MapLeadToDto.
/// </summary>
public static class LeadMapper
{
    public static LeadDto MapToDto(Lead lead)
    {
        return new LeadDto
        {
            Id = lead.Id,
            SupplierProfileId = lead.SupplierProfileId,
            BuyerOrganizationId = lead.BuyerOrganizationId,
            BuyerUserId = lead.BuyerUserId,
            ContactName = lead.ContactName,
            ContactEmail = lead.ContactEmail,
            ContactPhone = lead.ContactPhone,
            CompanyName = lead.CompanyName,
            Message = lead.Message,
            Status = lead.Status.ToString(),
            LeadType = lead.LeadType,
            IpAddress = lead.IpAddress,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }
}
