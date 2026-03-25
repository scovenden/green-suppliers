using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Policy = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly GreenSuppliersDbContext _context;

    public AdminDashboardController(GreenSuppliersDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var totalSuppliers = await _context.SupplierProfiles
            .CountAsync(s => !s.IsDeleted, ct);

        var verifiedSuppliers = await _context.SupplierProfiles
            .CountAsync(s => !s.IsDeleted && s.VerificationStatus == VerificationStatus.Verified, ct);

        var newLeads = await _context.Leads
            .CountAsync(l => l.Status == LeadStatus.New, ct);

        var pendingCerts = await _context.SupplierCertifications
            .CountAsync(c => c.Status == CertificationStatus.Pending, ct);

        var stats = new
        {
            totalSuppliers,
            verifiedSuppliers,
            newLeads,
            pendingCertifications = pendingCerts
        };

        return Ok(ApiResponse<object>.Ok(stats));
    }

    [HttpGet("activity")]
    public async Task<IActionResult> GetActivity(CancellationToken ct)
    {
        var events = await _context.AuditEvents
            .OrderByDescending(e => e.CreatedAt)
            .Take(20)
            .Select(e => new
            {
                e.Id,
                e.Action,
                e.EntityType,
                e.EntityId,
                description = e.Action + " on " + e.EntityType,
                e.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(events));
    }
}
