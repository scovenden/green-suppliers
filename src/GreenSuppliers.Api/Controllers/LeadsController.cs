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
        var lead = await _leadService.CreateLeadAsync(request, ipAddress);

        return StatusCode(201, ApiResponse<LeadDto>.Ok(lead));
    }
}
