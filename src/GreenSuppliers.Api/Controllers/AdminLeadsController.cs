using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/leads")]
[Authorize(Policy = "Admin")]
public class AdminLeadsController : ControllerBase
{
    private readonly LeadService _leadService;

    public AdminLeadsController(LeadService leadService)
    {
        _leadService = leadService;
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = await _leadService.GetAllAsync(page, pageSize, status);

        var meta = new PaginationMeta(result.Page, result.PageSize, result.Total, result.TotalPages);
        return Ok(ApiResponse<List<LeadDto>>.Ok(result.Items, meta));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLeadStatusRequest request, CancellationToken ct)
    {
        var adminUserId = GetAdminUserId();
        var success = await _leadService.UpdateStatusAsync(id, request.Status, adminUserId);

        if (!success)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Lead not found or invalid status."));

        return Ok(ApiResponse<object>.Ok(new { id, status = request.Status }));
    }

    private Guid GetAdminUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : Guid.Empty;
    }
}

public record UpdateLeadStatusRequest(string Status);
