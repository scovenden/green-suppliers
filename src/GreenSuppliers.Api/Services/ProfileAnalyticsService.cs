using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class ProfileAnalyticsService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly ILogger<ProfileAnalyticsService> _logger;

    public ProfileAnalyticsService(GreenSuppliersDbContext context, ILogger<ProfileAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Records a profile view. Designed to be called fire-and-forget so it does not
    /// slow down the profile GET response.
    /// </summary>
    public async Task RecordViewAsync(
        Guid supplierProfileId, string? viewerIp, Guid? viewerUserId, string? referrer, CancellationToken ct)
    {
        try
        {
            _context.ProfileViews.Add(new ProfileView
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = supplierProfileId,
                ViewerIp = viewerIp?[..Math.Min(viewerIp.Length, 45)],
                ViewerUserId = viewerUserId,
                Referrer = referrer?[..Math.Min(referrer.Length, 2000)],
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            // Fire-and-forget: log but do not throw — view recording must never break profile retrieval
            _logger.LogWarning(ex, "Failed to record profile view for SupplierProfileId={SupplierProfileId}", supplierProfileId);
        }
    }

    /// <summary>
    /// Returns analytics for a supplier profile: total views, views this month,
    /// views by day (last 30 days), total leads, leads by month.
    /// </summary>
    public async Task<ProfileAnalyticsDto> GetAnalyticsAsync(Guid supplierProfileId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var thirtyDaysAgo = now.AddDays(-30);
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Total views
        var totalViews = await _context.ProfileViews
            .CountAsync(pv => pv.SupplierProfileId == supplierProfileId, ct);

        // Views this month
        var viewsThisMonth = await _context.ProfileViews
            .CountAsync(pv => pv.SupplierProfileId == supplierProfileId && pv.CreatedAt >= startOfMonth, ct);

        // Views by day (last 30 days)
        var viewsByDay = await _context.ProfileViews
            .Where(pv => pv.SupplierProfileId == supplierProfileId && pv.CreatedAt >= thirtyDaysAgo)
            .GroupBy(pv => pv.CreatedAt.Date)
            .Select(g => new ViewsByDayDto
            {
                Date = DateOnly.FromDateTime(g.Key),
                Count = g.Count()
            })
            .OrderBy(v => v.Date)
            .ToListAsync(ct);

        // Total leads
        var totalLeads = await _context.Leads
            .CountAsync(l => l.SupplierProfileId == supplierProfileId, ct);

        // Leads by month (last 12 months)
        var twelveMonthsAgo = now.AddMonths(-12);
        var leadsByMonth = await _context.Leads
            .Where(l => l.SupplierProfileId == supplierProfileId && l.CreatedAt >= twelveMonthsAgo)
            .GroupBy(l => new { l.CreatedAt.Year, l.CreatedAt.Month })
            .Select(g => new LeadsByMonthDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Count = g.Count()
            })
            .OrderBy(l => l.Year).ThenBy(l => l.Month)
            .ToListAsync(ct);

        return new ProfileAnalyticsDto
        {
            TotalViews = totalViews,
            ViewsThisMonth = viewsThisMonth,
            ViewsByDay = viewsByDay,
            TotalLeads = totalLeads,
            LeadsByMonth = leadsByMonth
        };
    }
}
