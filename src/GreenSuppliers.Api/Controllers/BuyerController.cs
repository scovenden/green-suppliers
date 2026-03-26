using GreenSuppliers.Api.Extensions;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/buyer/me")]
[Authorize(Policy = "Buyer")]
public class BuyerController : ControllerBase
{
    private readonly BuyerService _buyerService;

    public BuyerController(BuyerService buyerService)
    {
        _buyerService = buyerService;
    }

    /// <summary>
    /// List the buyer's saved suppliers (paginated).
    /// </summary>
    [HttpGet("saved-suppliers")]
    public async Task<IActionResult> GetSavedSuppliers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var userId = User.GetUserId();
        var result = await _buyerService.GetSavedSuppliersAsync(userId, page, pageSize, ct);

        return Ok(ApiResponse<List<SupplierSummaryDto>>.Ok(
            result.Items,
            new PaginationMeta(result.Page, result.PageSize, result.Total, result.TotalPages)));
    }

    /// <summary>
    /// Save a supplier to the buyer's list.
    /// </summary>
    [HttpPost("saved-suppliers")]
    public async Task<IActionResult> SaveSupplier([FromBody] SaveSupplierRequest request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var success = await _buyerService.SaveSupplierAsync(userId, request.SupplierProfileId, ct);

        if (!success)
            return Conflict(ApiResponse<object>.Fail("SAVE_FAILED",
                "Supplier could not be saved. It may already be saved, not exist, or not be published."));

        return StatusCode(201, ApiResponse<object>.Ok(new { saved = true }));
    }

    /// <summary>
    /// Remove a supplier from the buyer's saved list.
    /// </summary>
    [HttpDelete("saved-suppliers/{id:guid}")]
    public async Task<IActionResult> UnsaveSupplier(Guid id, CancellationToken ct)
    {
        var userId = User.GetUserId();
        var success = await _buyerService.UnsaveSupplierAsync(userId, id, ct);

        if (!success)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Saved supplier not found."));

        return NoContent();
    }

    /// <summary>
    /// List the buyer's sent inquiries (paginated).
    /// </summary>
    [HttpGet("leads")]
    public async Task<IActionResult> GetLeads(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var userId = User.GetUserId();
        var result = await _buyerService.GetBuyerLeadsAsync(userId, page, pageSize, ct);

        return Ok(ApiResponse<List<LeadDto>>.Ok(
            result.Items,
            new PaginationMeta(result.Page, result.PageSize, result.Total, result.TotalPages)));
    }

    /// <summary>
    /// Get buyer dashboard stats (saved count, inquiry count, response count).
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var userId = User.GetUserId();
        var dashboard = await _buyerService.GetDashboardAsync(userId, ct);

        return Ok(ApiResponse<BuyerDashboardDto>.Ok(dashboard));
    }
}
