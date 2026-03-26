using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Extensions;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/suppliers")]
[Authorize(Policy = "Admin")]
public class AdminSuppliersController : ControllerBase
{
    private readonly SupplierService _supplierService;
    private readonly GreenSuppliersDbContext _db;

    public AdminSuppliersController(SupplierService supplierService, GreenSuppliersDbContext db)
    {
        _supplierService = supplierService;
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.SupplierProfiles
            .AsNoTracking()
            .Include(p => p.Organization)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new AdminSupplierListItem
            {
                Id = p.Id,
                Slug = p.Slug,
                CompanyName = p.Organization.Name,
                TradingName = p.TradingName,
                CountryCode = p.CountryCode,
                City = p.City,
                VerificationStatus = p.VerificationStatus.ToString(),
                EsgLevel = p.EsgLevel.ToString(),
                EsgScore = p.EsgScore,
                IsPublished = p.IsPublished,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        var meta = new PaginationMeta(page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
        return Ok(ApiResponse<List<AdminSupplierListItem>>.Ok(items, meta));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        var adminUserId = User.GetUserId();
        var profile = await _supplierService.CreateAsync(request, adminUserId, ct);

        return StatusCode(201, ApiResponse<SupplierProfileDto>.Ok(profile));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        var adminUserId = User.GetUserId();
        var profile = await _supplierService.UpdateAsync(id, request, adminUserId, ct);

        if (profile is null)
            return NotFound(ApiResponse<SupplierProfileDto>.Fail("NOT_FOUND", "Supplier not found."));

        return Ok(ApiResponse<SupplierProfileDto>.Ok(profile));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<VerificationStatus>(request.Status, ignoreCase: true, out var status))
            return BadRequest(ApiResponse<object>.Fail("INVALID_STATUS", "Invalid verification status value."));

        var adminUserId = User.GetUserId();
        var success = await _supplierService.SetVerificationStatusAsync(id, status, request.Reason, adminUserId, ct);

        if (!success)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Supplier not found."));

        return Ok(ApiResponse<object>.Ok(new { id, status = status.ToString() }));
    }

    [HttpPatch("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id, [FromBody] PublishRequest request, CancellationToken ct)
    {
        var adminUserId = User.GetUserId();
        var success = await _supplierService.SetPublishedAsync(id, request.Published, adminUserId, ct);

        if (!success)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Supplier not found."));

        return Ok(ApiResponse<object>.Ok(new { id, published = request.Published }));
    }

    [HttpPost("{id:guid}/rescore")]
    public async Task<IActionResult> Rescore(Guid id, CancellationToken ct)
    {
        var success = await _supplierService.RescoreAsync(id, ct);

        if (!success)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Supplier not found."));

        var profile = await _supplierService.GetByIdAsync(id, ct);
        return Ok(ApiResponse<SupplierProfileDto>.Ok(profile!));
    }

}

public record ChangeStatusRequest(string Status, string? Reason);
public record PublishRequest(bool Published);

public class AdminSupplierListItem
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? TradingName { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? City { get; set; }
    public string VerificationStatus { get; set; } = string.Empty;
    public string EsgLevel { get; set; } = string.Empty;
    public int EsgScore { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAt { get; set; }
}
