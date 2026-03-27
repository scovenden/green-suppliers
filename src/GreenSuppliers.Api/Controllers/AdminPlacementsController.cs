using GreenSuppliers.Api.Extensions;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/placements")]
[Authorize(Policy = "Admin")]
public class AdminPlacementsController : ControllerBase
{
    private readonly PlacementService _placementService;

    public AdminPlacementsController(PlacementService placementService)
    {
        _placementService = placementService;
    }

    /// <summary>
    /// List all sponsored placements (paginated). Admin only.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _placementService.GetAllAsync(page, pageSize, ct);
        var meta = new PaginationMeta(result.Page, result.PageSize, result.Total, result.TotalPages);
        return Ok(ApiResponse<List<PlacementDto>>.Ok(result.Items, meta));
    }

    /// <summary>
    /// Create a new sponsored placement. Admin only.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePlacementRequest request, CancellationToken ct)
    {
        var adminUserId = User.GetUserId();

        try
        {
            var placement = await _placementService.CreateAsync(request, adminUserId, ct);
            return StatusCode(201, ApiResponse<PlacementDto>.Ok(placement));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<PlacementDto>.Fail("NOT_FOUND", ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PlacementDto>.Fail("VALIDATION_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// Update an existing sponsored placement (partial update). Admin only.
    /// </summary>
    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePlacementRequest request, CancellationToken ct)
    {
        var adminUserId = User.GetUserId();

        try
        {
            var placement = await _placementService.UpdateAsync(id, request, adminUserId, ct);

            if (placement is null)
                return NotFound(ApiResponse<PlacementDto>.Fail("NOT_FOUND", "Placement not found."));

            return Ok(ApiResponse<PlacementDto>.Ok(placement));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<PlacementDto>.Fail("VALIDATION_ERROR", ex.Message));
        }
    }

    /// <summary>
    /// Record an impression for a placement. Admin utility endpoint.
    /// </summary>
    [HttpPost("{id:guid}/impressions")]
    public async Task<IActionResult> RecordImpression(Guid id, CancellationToken ct)
    {
        await _placementService.RecordImpressionAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { recorded = true }));
    }

    /// <summary>
    /// Record a click for a placement. Admin utility endpoint.
    /// </summary>
    [HttpPost("{id:guid}/clicks")]
    public async Task<IActionResult> RecordClick(Guid id, CancellationToken ct)
    {
        await _placementService.RecordClickAsync(id, ct);
        return Ok(ApiResponse<object>.Ok(new { recorded = true }));
    }
}
