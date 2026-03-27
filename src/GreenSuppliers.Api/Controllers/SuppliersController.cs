using System.Security.Claims;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/suppliers")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierSearchService _searchService;
    private readonly SupplierService _supplierService;
    private readonly GreenSuppliersDbContext _db;
    private readonly ProfileAnalyticsService _analyticsService;
    private readonly PlacementService _placementService;

    public SuppliersController(
        ISupplierSearchService searchService,
        SupplierService supplierService,
        GreenSuppliersDbContext db,
        ProfileAnalyticsService analyticsService,
        PlacementService placementService)
    {
        _searchService = searchService;
        _supplierService = supplierService;
        _db = db;
        _analyticsService = analyticsService;
        _placementService = placementService;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SupplierSearchQuery query, CancellationToken ct)
    {
        var result = await _searchService.SearchAsync(query, ct);

        var meta = new PaginationMeta(result.Page, result.PageSize, result.Total, result.TotalPages);
        return Ok(ApiResponse<List<SupplierSearchResult>>.Ok(result.Items, meta));
    }

    /// <summary>
    /// Returns currently active sponsored/featured placements. Public endpoint.
    /// </summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(CancellationToken ct)
    {
        var placements = await _placementService.GetFeaturedAsync(ct);
        return Ok(ApiResponse<List<PlacementDto>>.Ok(placements));
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var profile = await _supplierService.GetBySlugAsync(slug, ct);

        if (profile is null)
            return NotFound(ApiResponse<SupplierProfileDto>.Fail("NOT_FOUND", "Supplier not found."));

        // Fire-and-forget: record profile view without blocking the response
        var viewerIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var viewerUserId = User.Identity?.IsAuthenticated == true
            ? Guid.TryParse(User.FindFirst("sub")?.Value, out var uid) ? uid : (Guid?)null
            : null;
        var referrer = HttpContext.Request.Headers.Referer.FirstOrDefault();

        _ = Task.Run(async () =>
        {
            // Use a separate scope so the DbContext is not shared with the request pipeline
            using var scope = HttpContext.RequestServices.CreateScope();
            var analytics = scope.ServiceProvider.GetRequiredService<ProfileAnalyticsService>();
            await analytics.RecordViewAsync(profile.Id, viewerIp, viewerUserId, referrer, CancellationToken.None);
        });

        return Ok(ApiResponse<SupplierProfileDto>.Ok(profile));
    }

    [HttpGet("{slug}/certifications")]
    public async Task<IActionResult> GetCertifications(string slug, CancellationToken ct)
    {
        var profile = await _db.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished && !p.IsDeleted, ct);

        if (profile is null)
            return NotFound(ApiResponse<List<CertificationDto>>.Fail("NOT_FOUND", "Supplier not found."));

        var certs = await _db.SupplierCertifications
            .AsNoTracking()
            .Include(c => c.CertificationType)
            .Where(c => c.SupplierProfileId == profile.Id)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CertificationDto
            {
                Id = c.Id,
                SupplierProfileId = c.SupplierProfileId,
                CertTypeName = c.CertificationType.Name,
                CertTypeSlug = c.CertificationType.Slug,
                CertificateNumber = c.CertificateNumber,
                IssuedAt = c.IssuedAt,
                ExpiresAt = c.ExpiresAt,
                Status = c.Status.ToString(),
                Notes = c.Notes
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<List<CertificationDto>>.Ok(certs));
    }
}
