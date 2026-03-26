using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class BuyerService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly AuditService _audit;

    public BuyerService(GreenSuppliersDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    public async Task<PagedResult<SupplierSummaryDto>> GetSavedSuppliersAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.SavedSuppliers
            .AsNoTracking()
            .Where(ss => ss.BuyerUserId == userId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(ss => ss.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ss => new SupplierSummaryDto
            {
                Id = ss.SupplierProfile.Id,
                Slug = ss.SupplierProfile.Slug,
                TradingName = ss.SupplierProfile.TradingName,
                ShortDescription = ss.SupplierProfile.ShortDescription,
                City = ss.SupplierProfile.City,
                CountryCode = ss.SupplierProfile.CountryCode,
                VerificationStatus = ss.SupplierProfile.VerificationStatus,
                EsgLevel = ss.SupplierProfile.EsgLevel,
                EsgScore = ss.SupplierProfile.EsgScore,
                LogoUrl = ss.SupplierProfile.LogoUrl,
                Industries = ss.SupplierProfile.SupplierIndustries
                    .Select(si => si.Industry.Name).ToList(),
                IsVerified = ss.SupplierProfile.VerificationStatus == VerificationStatus.Verified
            })
            .ToListAsync(ct);

        return new PagedResult<SupplierSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<bool> SaveSupplierAsync(Guid userId, Guid supplierProfileId, CancellationToken ct)
    {
        // Verify the supplier profile exists and is published
        var profileExists = await _context.SupplierProfiles
            .AnyAsync(sp => sp.Id == supplierProfileId && !sp.IsDeleted && sp.IsPublished, ct);

        if (!profileExists)
            return false;

        // Check for duplicate
        var alreadySaved = await _context.SavedSuppliers
            .AnyAsync(ss => ss.BuyerUserId == userId && ss.SupplierProfileId == supplierProfileId, ct);

        if (alreadySaved)
            return false;

        var savedSupplier = new SavedSupplier
        {
            Id = Guid.NewGuid(),
            BuyerUserId = userId,
            SupplierProfileId = supplierProfileId,
            CreatedAt = DateTime.UtcNow
        };

        _context.SavedSuppliers.Add(savedSupplier);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "SupplierSaved", "SavedSupplier", savedSupplier.Id, ct: ct);

        return true;
    }

    public async Task<bool> UnsaveSupplierAsync(Guid userId, Guid savedSupplierId, CancellationToken ct)
    {
        var savedSupplier = await _context.SavedSuppliers
            .FirstOrDefaultAsync(ss => ss.Id == savedSupplierId && ss.BuyerUserId == userId, ct);

        if (savedSupplier is null)
            return false;

        _context.SavedSuppliers.Remove(savedSupplier);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(userId, "SupplierUnsaved", "SavedSupplier", savedSupplierId, ct: ct);

        return true;
    }

    public async Task<PagedResult<LeadDto>> GetBuyerLeadsAsync(Guid userId, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Leads
            .AsNoTracking()
            .Where(l => l.BuyerUserId == userId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<LeadDto>
        {
            Items = items.Select(MapLeadToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<BuyerDashboardDto> GetDashboardAsync(Guid userId, CancellationToken ct)
    {
        var savedCount = await _context.SavedSuppliers
            .CountAsync(ss => ss.BuyerUserId == userId, ct);

        var inquirySentCount = await _context.Leads
            .CountAsync(l => l.BuyerUserId == userId, ct);

        var inquiryRespondedCount = await _context.Leads
            .CountAsync(l => l.BuyerUserId == userId && l.Status != LeadStatus.New, ct);

        return new BuyerDashboardDto
        {
            SavedSupplierCount = savedCount,
            InquirySentCount = inquirySentCount,
            InquiryRespondedCount = inquiryRespondedCount
        };
    }

    private static LeadDto MapLeadToDto(Lead lead)
    {
        return new LeadDto
        {
            Id = lead.Id,
            SupplierProfileId = lead.SupplierProfileId,
            BuyerOrganizationId = lead.BuyerOrganizationId,
            BuyerUserId = lead.BuyerUserId,
            ContactName = lead.ContactName,
            ContactEmail = lead.ContactEmail,
            ContactPhone = lead.ContactPhone,
            CompanyName = lead.CompanyName,
            Message = lead.Message,
            Status = lead.Status.ToString(),
            LeadType = lead.LeadType,
            IpAddress = lead.IpAddress,
            CreatedAt = lead.CreatedAt,
            UpdatedAt = lead.UpdatedAt
        };
    }
}
