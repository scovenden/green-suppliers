using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/leads")]
public class LeadsController : ControllerBase
{
    private readonly LeadService _leadService;

    public LeadsController(LeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] LeadRequest request, CancellationToken ct)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // If the request has a valid JWT with a buyer role, automatically set buyer identity
        Guid? buyerUserId = null;
        Guid? buyerOrgId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var roleClaim = User.FindFirst("role")?.Value;
            if (roleClaim == "Buyer")
            {
                var subClaim = User.FindFirst("sub")?.Value;
                var orgClaim = User.FindFirst("organizationId")?.Value;

                if (subClaim is not null && Guid.TryParse(subClaim, out var parsedUserId))
                    buyerUserId = parsedUserId;

                if (orgClaim is not null && Guid.TryParse(orgClaim, out var parsedOrgId))
                    buyerOrgId = parsedOrgId;
            }
        }

        var lead = await _leadService.CreateLeadAsync(request, ipAddress, buyerUserId, buyerOrgId, ct);

        return StatusCode(201, ApiResponse<LeadDto>.Ok(lead));
    }
}
