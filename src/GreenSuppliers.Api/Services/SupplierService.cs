using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Helpers;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class SupplierService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly ScoringRunner _scoringRunner;
    private readonly AuditService _audit;

    public SupplierService(
        GreenSuppliersDbContext context,
        ScoringRunner scoringRunner,
        AuditService audit)
    {
        _context = context;
        _scoringRunner = scoringRunner;
        _audit = audit;
    }

    public async Task<SupplierProfileDto> CreateAsync(CreateSupplierRequest request, Guid adminUserId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Create Organization
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.CompanyName,
            CountryCode = request.CountryCode,
            City = request.City,
            Province = request.Province,
            Website = request.Website,
            Phone = request.Phone,
            Email = request.Email,
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.Organizations.Add(org);

        // Generate slug
        var slugSource = !string.IsNullOrWhiteSpace(request.TradingName)
            ? request.TradingName
            : request.CompanyName;
        var slug = await GenerateUniqueSlugAsync(slugSource, ct);

        // Create SupplierProfile
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Slug = slug,
            TradingName = request.TradingName,
            Description = request.Description,
            ShortDescription = request.ShortDescription,
            CountryCode = request.CountryCode,
            City = request.City,
            Province = request.Province,
            RenewableEnergyPercent = request.RenewableEnergyPercent,
            WasteRecyclingPercent = request.WasteRecyclingPercent,
            CarbonReporting = request.CarbonReporting,
            WaterManagement = request.WaterManagement,
            SustainablePackaging = request.SustainablePackaging,
            IsPublished = true,
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.SupplierProfiles.Add(profile);

        // Link industries
        foreach (var industryId in request.IndustryIds)
        {
            _context.Set<SupplierIndustry>().Add(new SupplierIndustry
            {
                SupplierProfileId = profile.Id,
                IndustryId = industryId
            });
        }

        // Link service tags
        foreach (var tagId in request.ServiceTagIds)
        {
            _context.Set<SupplierServiceTag>().Add(new SupplierServiceTag
            {
                SupplierProfileId = profile.Id,
                ServiceTagId = tagId
            });
        }

        await _context.SaveChangesAsync(ct);

        // Run ESG scoring + verification
        await _scoringRunner.RunScoringAsync(profile, ct);

        // Write audit log
        await _audit.LogAsync(adminUserId, "SupplierCreated", "SupplierProfile", profile.Id, ct: ct);

        // Return full DTO
        return await SupplierProfileMapper.BuildProfileDtoAsync(_context, profile.Id, ct);
    }

    public async Task<SupplierProfileDto?> UpdateAsync(Guid id, UpdateSupplierRequest request, Guid adminUserId, CancellationToken ct = default)
    {
        var profile = await _context.SupplierProfiles
            .Include(p => p.Organization)
            .Include(p => p.SupplierIndustries)
            .Include(p => p.SupplierServiceTags)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (profile is null)
            return null;

        var now = DateTime.UtcNow;

        // Update organization
        profile.Organization.Name = request.CompanyName;
        profile.Organization.CountryCode = request.CountryCode;
        profile.Organization.City = request.City;
        profile.Organization.Province = request.Province;
        profile.Organization.Website = request.Website;
        profile.Organization.Phone = request.Phone;
        profile.Organization.Email = request.Email;
        profile.Organization.UpdatedAt = now;

        // Update profile fields
        profile.TradingName = request.TradingName;
        profile.Description = request.Description;
        profile.ShortDescription = request.ShortDescription;
        profile.CountryCode = request.CountryCode;
        profile.City = request.City;
        profile.Province = request.Province;
        profile.RenewableEnergyPercent = request.RenewableEnergyPercent;
        profile.WasteRecyclingPercent = request.WasteRecyclingPercent;
        profile.CarbonReporting = request.CarbonReporting;
        profile.WaterManagement = request.WaterManagement;
        profile.SustainablePackaging = request.SustainablePackaging;
        profile.UpdatedAt = now;

        // Re-link industries
        _context.Set<SupplierIndustry>().RemoveRange(profile.SupplierIndustries);
        foreach (var industryId in request.IndustryIds)
        {
            _context.Set<SupplierIndustry>().Add(new SupplierIndustry
            {
                SupplierProfileId = profile.Id,
                IndustryId = industryId
            });
        }

        // Re-link service tags
        _context.Set<SupplierServiceTag>().RemoveRange(profile.SupplierServiceTags);
        foreach (var tagId in request.ServiceTagIds)
        {
            _context.Set<SupplierServiceTag>().Add(new SupplierServiceTag
            {
                SupplierProfileId = profile.Id,
                ServiceTagId = tagId
            });
        }

        await _context.SaveChangesAsync(ct);

        // Re-run scoring
        await _scoringRunner.RunScoringAsync(profile, ct);

        // Write audit log
        await _audit.LogAsync(adminUserId, "SupplierUpdated", "SupplierProfile", profile.Id, ct: ct);

        return await SupplierProfileMapper.BuildProfileDtoAsync(_context, profile.Id, ct);
    }

    public async Task<SupplierProfileDto?> GetBySlugAsync(string slug, CancellationToken ct = default)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished && !p.IsDeleted, ct);

        if (profile is null)
            return null;

        return await SupplierProfileMapper.BuildProfileDtoAsync(_context, profile.Id, ct);
    }

    public async Task<SupplierProfileDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (profile is null)
            return null;

        return await SupplierProfileMapper.BuildProfileDtoAsync(_context, id, ct);
    }

    public async Task<bool> SetVerificationStatusAsync(Guid id, VerificationStatus status, string? reason, Guid adminUserId, CancellationToken ct = default)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (profile is null)
            return false;

        profile.VerificationStatus = status;
        profile.FlaggedReason = status == VerificationStatus.Flagged ? reason : null;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(adminUserId, "VerificationStatusChanged", "SupplierProfile", id,
            newValues: $"{{\"status\":\"{status}\",\"reason\":\"{reason}\"}}", ct: ct);

        return true;
    }

    public async Task<bool> SetPublishedAsync(Guid id, bool published, Guid adminUserId, CancellationToken ct = default)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (profile is null)
            return false;

        profile.IsPublished = published;
        profile.PublishedAt = published ? DateTime.UtcNow : null;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(adminUserId, published ? "SupplierPublished" : "SupplierUnpublished",
            "SupplierProfile", id, ct: ct);

        return true;
    }

    public async Task<bool> RescoreAsync(Guid id, CancellationToken ct = default)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

        if (profile is null)
            return false;

        await _scoringRunner.RunScoringAsync(profile, ct);
        return true;
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken ct = default)
    {
        var baseSlug = SlugHelper.Slugify(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _context.SupplierProfiles.AnyAsync(p => p.Slug == slug, ct))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }
}
