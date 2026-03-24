using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/certifications")]
[Authorize(Policy = "Admin")]
public class AdminCertificationsController : ControllerBase
{
    private readonly GreenSuppliersDbContext _db;
    private readonly AuditService _audit;
    private readonly SupplierService _supplierService;

    public AdminCertificationsController(
        GreenSuppliersDbContext db,
        AuditService audit,
        SupplierService supplierService)
    {
        _db = db;
        _audit = audit;
        _supplierService = supplierService;
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

        var query = _db.SupplierCertifications
            .AsNoTracking()
            .Include(c => c.CertificationType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CertificationStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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

        var meta = new PaginationMeta(page, pageSize, total, (int)Math.Ceiling(total / (double)pageSize));
        return Ok(ApiResponse<List<CertificationDto>>.Ok(items, meta));
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> ReviewCertification(Guid id, [FromBody] ReviewCertRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<CertificationStatus>(request.Status, ignoreCase: true, out var newStatus))
            return BadRequest(ApiResponse<object>.Fail("INVALID_STATUS", "Invalid certification status value."));

        if (newStatus != CertificationStatus.Accepted && newStatus != CertificationStatus.Rejected)
            return BadRequest(ApiResponse<object>.Fail("INVALID_STATUS", "Status must be Accepted or Rejected."));

        var cert = await _db.SupplierCertifications.FindAsync(new object[] { id }, ct);
        if (cert is null)
            return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Certification not found."));

        var adminUserId = GetAdminUserId();
        var oldStatus = cert.Status.ToString();

        cert.Status = newStatus;
        cert.Notes = request.Notes;
        cert.VerifiedByUserId = adminUserId;
        cert.VerifiedAt = DateTime.UtcNow;
        cert.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _audit.LogAsync(adminUserId, "CertificationReviewed", "SupplierCertification", id,
            oldValues: $"{{\"status\":\"{oldStatus}\"}}",
            newValues: $"{{\"status\":\"{newStatus}\"}}");

        // Re-score the supplier after certification status change
        await _supplierService.RescoreAsync(cert.SupplierProfileId);

        return Ok(ApiResponse<object>.Ok(new { id, status = newStatus.ToString() }));
    }

    private Guid GetAdminUserId()
    {
        var sub = User.FindFirst("sub")?.Value;
        return Guid.TryParse(sub, out var userId) ? userId : Guid.Empty;
    }
}

public record ReviewCertRequest(string Status, string? Notes);
