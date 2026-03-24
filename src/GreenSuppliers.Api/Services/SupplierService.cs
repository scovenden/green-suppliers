using System.Text.RegularExpressions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class SupplierService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly EsgScoringService _esgScoring;
    private readonly VerificationService _verification;
    private readonly AuditService _audit;

    public SupplierService(
        GreenSuppliersDbContext context,
        EsgScoringService esgScoring,
        VerificationService verification,
        AuditService audit)
    {
        _context = context;
        _esgScoring = esgScoring;
        _verification = verification;
        _audit = audit;
    }

    public async Task<SupplierProfileDto> CreateAsync(CreateSupplierRequest request, Guid adminUserId)
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
        var slug = await GenerateUniqueSlugAsync(slugSource);

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

        await _context.SaveChangesAsync();

        // Run ESG scoring + verification
        await RunScoringAsync(profile);

        // Write audit log
        await _audit.LogAsync(adminUserId, "SupplierCreated", "SupplierProfile", profile.Id);

        // Return full DTO
        return await BuildProfileDtoAsync(profile.Id);
    }

    public async Task<SupplierProfileDto?> UpdateAsync(Guid id, UpdateSupplierRequest request, Guid adminUserId)
    {
        var profile = await _context.SupplierProfiles
            .Include(p => p.Organization)
            .Include(p => p.SupplierIndustries)
            .Include(p => p.SupplierServiceTags)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

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

        await _context.SaveChangesAsync();

        // Re-run scoring
        await RunScoringAsync(profile);

        // Write audit log
        await _audit.LogAsync(adminUserId, "SupplierUpdated", "SupplierProfile", profile.Id);

        return await BuildProfileDtoAsync(profile.Id);
    }

    public async Task<SupplierProfileDto?> GetBySlugAsync(string slug)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished && !p.IsDeleted);

        if (profile is null)
            return null;

        return await BuildProfileDtoAsync(profile.Id);
    }

    public async Task<SupplierProfileDto?> GetByIdAsync(Guid id)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (profile is null)
            return null;

        return await BuildProfileDtoAsync(id);
    }

    public async Task<bool> SetVerificationStatusAsync(Guid id, VerificationStatus status, string? reason, Guid adminUserId)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (profile is null)
            return false;

        profile.VerificationStatus = status;
        profile.FlaggedReason = status == VerificationStatus.Flagged ? reason : null;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _audit.LogAsync(adminUserId, "VerificationStatusChanged", "SupplierProfile", id,
            newValues: $"{{\"status\":\"{status}\",\"reason\":\"{reason}\"}}");

        return true;
    }

    public async Task<bool> SetPublishedAsync(Guid id, bool published, Guid adminUserId)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (profile is null)
            return false;

        profile.IsPublished = published;
        profile.PublishedAt = published ? DateTime.UtcNow : null;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _audit.LogAsync(adminUserId, published ? "SupplierPublished" : "SupplierUnpublished",
            "SupplierProfile", id);

        return true;
    }

    public async Task<bool> RescoreAsync(Guid id)
    {
        var profile = await _context.SupplierProfiles
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (profile is null)
            return false;

        await RunScoringAsync(profile);
        return true;
    }

    private async Task RunScoringAsync(SupplierProfile profile)
    {
        var certs = await _context.SupplierCertifications
            .Where(c => c.SupplierProfileId == profile.Id)
            .ToListAsync();

        var esgResult = _esgScoring.CalculateScore(profile, certs);
        profile.EsgLevel = esgResult.Level;
        profile.EsgScore = esgResult.NumericScore;

        var verificationStatus = _verification.Evaluate(profile, certs);
        profile.VerificationStatus = verificationStatus;

        profile.LastScoredAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private async Task<SupplierProfileDto> BuildProfileDtoAsync(Guid profileId)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .Include(p => p.Organization)
            .Include(p => p.SupplierIndustries).ThenInclude(si => si.Industry)
            .Include(p => p.SupplierServiceTags).ThenInclude(sst => sst.ServiceTag)
            .Include(p => p.Certifications).ThenInclude(c => c.CertificationType)
            .FirstAsync(p => p.Id == profileId);

        return MapToDto(profile);
    }

    private static SupplierProfileDto MapToDto(SupplierProfile profile)
    {
        return new SupplierProfileDto
        {
            Id = profile.Id,
            OrganizationId = profile.OrganizationId,
            OrganizationName = profile.Organization.Name,
            Slug = profile.Slug,
            TradingName = profile.TradingName,
            Description = profile.Description,
            ShortDescription = profile.ShortDescription,
            LogoUrl = profile.LogoUrl,
            BannerUrl = profile.BannerUrl,
            YearFounded = profile.YearFounded,
            EmployeeCount = profile.EmployeeCount,
            BbbeeLevel = profile.BbbeeLevel,
            CountryCode = profile.CountryCode,
            City = profile.City,
            Province = profile.Province,
            Website = profile.Organization.Website,
            Phone = profile.Organization.Phone,
            Email = profile.Organization.Email,
            RenewableEnergyPercent = profile.RenewableEnergyPercent,
            WasteRecyclingPercent = profile.WasteRecyclingPercent,
            CarbonReporting = profile.CarbonReporting,
            WaterManagement = profile.WaterManagement,
            SustainablePackaging = profile.SustainablePackaging,
            VerificationStatus = profile.VerificationStatus,
            EsgLevel = profile.EsgLevel,
            EsgScore = profile.EsgScore,
            IsPublished = profile.IsPublished,
            PublishedAt = profile.PublishedAt,
            FlaggedReason = profile.FlaggedReason,
            LastScoredAt = profile.LastScoredAt,
            IsDeleted = profile.IsDeleted,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt,
            IsVerified = profile.VerificationStatus == VerificationStatus.Verified,
            Industries = profile.SupplierIndustries.Select(si => new SupplierIndustryDto
            {
                Id = si.Industry.Id,
                Name = si.Industry.Name,
                Slug = si.Industry.Slug
            }).ToList(),
            ServiceTags = profile.SupplierServiceTags.Select(sst => new SupplierServiceTagDto
            {
                Id = sst.ServiceTag.Id,
                Name = sst.ServiceTag.Name,
                Slug = sst.ServiceTag.Slug
            }).ToList(),
            Certifications = profile.Certifications.Select(c => new SupplierCertificationDto
            {
                Id = c.Id,
                CertificationTypeName = c.CertificationType.Name,
                CertificateNumber = c.CertificateNumber,
                IssuedAt = c.IssuedAt,
                ExpiresAt = c.ExpiresAt,
                Status = c.Status.ToString()
            }).ToList()
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(string name)
    {
        var baseSlug = Slugify(name);
        var slug = baseSlug;
        var suffix = 2;

        while (await _context.SupplierProfiles.AnyAsync(p => p.Slug == slug))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string Slugify(string input)
    {
        var slug = input.ToLowerInvariant().Trim();
        // Replace spaces and underscores with hyphens
        slug = Regex.Replace(slug, @"[\s_]+", "-");
        // Remove special characters (keep letters, digits, hyphens)
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        // Collapse multiple hyphens
        slug = Regex.Replace(slug, @"-{2,}", "-");
        // Trim leading/trailing hyphens
        slug = slug.Trim('-');
        return slug;
    }
}
