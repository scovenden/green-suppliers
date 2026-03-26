using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Helpers;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSuppliers.Api.Services;

/// <summary>
/// Handles lead-related operations for the authenticated supplier's own profile.
/// Extracted from SupplierMeService to reduce file size and improve cohesion.
/// Depends on SupplierMeService.GetProfileIdByOrgAsync for org-to-profile resolution.
/// </summary>
public class SupplierMeLeadService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly SupplierMeService _supplierMeService;
    private readonly AuditService _audit;
    private readonly ILogger<SupplierMeLeadService> _logger;

    public SupplierMeLeadService(
        GreenSuppliersDbContext context,
        SupplierMeService supplierMeService,
        AuditService audit,
        ILogger<SupplierMeLeadService> logger)
    {
        _context = context;
        _supplierMeService = supplierMeService;
        _audit = audit;
        _logger = logger;
    }

    public async Task<PagedResult<LeadDto>> GetLeadsAsync(Guid orgId, int page, int pageSize, string? status, CancellationToken ct)
    {
        var profileId = await _supplierMeService.GetProfileIdByOrgAsync(orgId, ct);

        if (profileId is null)
            return new PagedResult<LeadDto> { Items = new(), Page = page, PageSize = pageSize, Total = 0 };

        var query = _context.Leads
            .AsNoTracking()
            .Where(l => l.SupplierProfileId == profileId.Value);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<LeadStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(l => l.Status == parsedStatus);
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<LeadDto>
        {
            Items = items.Select(LeadMapper.MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<LeadDto?> GetLeadDetailAsync(Guid orgId, Guid leadId, CancellationToken ct)
    {
        var profileId = await _supplierMeService.GetProfileIdByOrgAsync(orgId, ct);

        if (profileId is null)
            return null;

        var lead = await _context.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && l.SupplierProfileId == profileId.Value, ct);

        if (lead is null)
            return null;

        return LeadMapper.MapToDto(lead);
    }

    public async Task<bool> UpdateLeadStatusAsync(Guid orgId, Guid leadId, string newStatus, Guid userId, CancellationToken ct)
    {
        if (!Enum.TryParse<LeadStatus>(newStatus, ignoreCase: true, out var parsedStatus))
            return false;

        var profileId = await _supplierMeService.GetProfileIdByOrgAsync(orgId, ct);

        if (profileId is null)
            return false;

        var lead = await _context.Leads
            .FirstOrDefaultAsync(l => l.Id == leadId && l.SupplierProfileId == profileId.Value, ct);

        if (lead is null)
            return false;

        // Enforce valid transitions: New -> Contacted, Contacted -> Closed
        var isValidTransition = (lead.Status == LeadStatus.New && parsedStatus == LeadStatus.Contacted)
                             || (lead.Status == LeadStatus.Contacted && parsedStatus == LeadStatus.Closed);

        if (!isValidTransition)
            return false;

        var oldStatus = lead.Status.ToString();
        lead.Status = parsedStatus;
        lead.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "SupplierLeadStatusChanged", "Lead", leadId,
            oldValues: $"{{\"status\":\"{oldStatus}\"}}",
            newValues: $"{{\"status\":\"{parsedStatus}\"}}", ct: ct);

        _logger.LogInformation(
            "Supplier lead status changed. LeadId={LeadId} OldStatus={OldStatus} NewStatus={NewStatus} UserId={UserId}",
            leadId, oldStatus, parsedStatus, userId);

        return true;
    }
}
