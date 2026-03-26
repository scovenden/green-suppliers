using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class SupplierMeService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly EsgScoringService _esgScoring;
    private readonly VerificationService _verification;
    private readonly AuditService _audit;

    public SupplierMeService(
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

    public async Task<SupplierProfileDto?> GetByOrganizationIdAsync(Guid orgId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted, ct);

        if (profile is null)
            return null;

        return await BuildProfileDtoAsync(profile.Id, ct);
    }

    public async Task<SupplierProfileDto?> UpdateOwnProfileAsync(Guid orgId, UpdateMyProfileRequest request, Guid userId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .Include(p => p.Organization)
            .Include(p => p.SupplierIndustries)
            .Include(p => p.SupplierServiceTags)
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted, ct);

        if (profile is null)
            return null;

        var now = DateTime.UtcNow;

        // Update only editable fields — CompanyName, CountryCode, VerificationStatus,
        // EsgLevel, IsPublished are NOT supplier-editable.
        profile.TradingName = request.TradingName;
        profile.Description = request.Description;
        profile.ShortDescription = request.ShortDescription;
        profile.YearFounded = request.YearFounded;
        profile.EmployeeCount = request.EmployeeCount;
        profile.BbbeeLevel = request.BbbeeLevel;
        profile.City = request.City;
        profile.Province = request.Province;
        profile.RenewableEnergyPercent = request.RenewableEnergyPercent;
        profile.WasteRecyclingPercent = request.WasteRecyclingPercent;
        profile.CarbonReporting = request.CarbonReporting;
        profile.WaterManagement = request.WaterManagement;
        profile.SustainablePackaging = request.SustainablePackaging;
        profile.UpdatedAt = now;

        // Update org contact fields (supplier can edit their own contact info)
        profile.Organization.Website = request.Website;
        profile.Organization.Phone = request.Phone;
        profile.Organization.Email = request.Email;
        profile.Organization.City = request.City;
        profile.Organization.Province = request.Province;
        profile.Organization.UpdatedAt = now;

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

        // Re-run ESG scoring + verification after profile update
        await RunScoringAsync(profile, ct);

        // Write audit log
        await _audit.LogAsync(userId, "SupplierSelfUpdated", "SupplierProfile", profile.Id, ct: ct);

        return await BuildProfileDtoAsync(profile.Id, ct);
    }

    public async Task<CertificationDto?> AddCertificationAsync(Guid orgId, AddCertificationRequest request, Guid userId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted, ct);

        if (profile is null)
            return null;

        // Verify the certification type exists
        var certTypeExists = await _context.CertificationTypes
            .AnyAsync(c => c.Id == request.CertificationTypeId, ct);

        if (!certTypeExists)
            return null;

        var now = DateTime.UtcNow;

        var certification = new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            CertificationTypeId = request.CertificationTypeId,
            CertificateNumber = request.CertificateNumber,
            IssuedAt = request.IssuedAt,
            ExpiresAt = request.ExpiresAt,
            DocumentId = request.DocumentId,
            Status = CertificationStatus.Pending, // Always Pending on supplier submission
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.SupplierCertifications.Add(certification);
        await _context.SaveChangesAsync(ct);

        // Re-run scoring (pending cert triggers verification status change)
        var trackedProfile = await _context.SupplierProfiles
            .FirstAsync(p => p.Id == profile.Id, ct);
        await RunScoringAsync(trackedProfile, ct);

        // Write audit log
        await _audit.LogAsync(userId, "CertificationSubmitted", "SupplierCertification", certification.Id, ct: ct);

        // Load the cert type for the DTO
        var certType = await _context.CertificationTypes
            .AsNoTracking()
            .FirstAsync(c => c.Id == request.CertificationTypeId, ct);

        return new CertificationDto
        {
            Id = certification.Id,
            SupplierProfileId = profile.Id,
            CertTypeName = certType.Name,
            CertTypeSlug = certType.Slug,
            CertificateNumber = certification.CertificateNumber,
            IssuedAt = certification.IssuedAt,
            ExpiresAt = certification.ExpiresAt,
            Status = certification.Status.ToString()
        };
    }

    public async Task<bool> RequestPublicationAsync(Guid orgId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .Include(p => p.Certifications)
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted, ct);

        if (profile is null)
            return false;

        // Check profile completeness — must be above a threshold to publish
        var certCount = profile.Certifications.Count;
        var completeness = CalculateCompleteness(profile, certCount);

        // Require at least 50% completeness to publish
        if (completeness < 50)
            return false;

        // Flagged profiles cannot be published by the supplier
        if (profile.VerificationStatus == VerificationStatus.Flagged)
            return false;

        profile.IsPublished = true;
        profile.PublishedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<SupplierDashboardDto> GetDashboardStatsAsync(Guid orgId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted, ct);

        if (profile is null)
        {
            return new SupplierDashboardDto();
        }

        var leadCount = await _context.Leads
            .CountAsync(l => l.SupplierProfileId == profile.Id, ct);

        var newLeadCount = await _context.Leads
            .CountAsync(l => l.SupplierProfileId == profile.Id && l.Status == LeadStatus.New, ct);

        var certificationCount = await _context.SupplierCertifications
            .CountAsync(c => c.SupplierProfileId == profile.Id, ct);

        var pendingCertCount = await _context.SupplierCertifications
            .CountAsync(c => c.SupplierProfileId == profile.Id && c.Status == CertificationStatus.Pending, ct);

        var completeness = CalculateCompleteness(profile, certificationCount);

        return new SupplierDashboardDto
        {
            LeadCount = leadCount,
            NewLeadCount = newLeadCount,
            CertificationCount = certificationCount,
            PendingCertCount = pendingCertCount,
            EsgLevel = profile.EsgLevel.ToString(),
            EsgScore = profile.EsgScore,
            VerificationStatus = profile.VerificationStatus.ToString(),
            IsPublished = profile.IsPublished,
            ProfileCompleteness = completeness
        };
    }

    public static int CalculateCompleteness(SupplierProfile profile, int certCount)
    {
        int score = 0;

        // BasicInfo (25%): tradingName, description, shortDescription, yearFounded
        if (!string.IsNullOrEmpty(profile.TradingName)) score += 7;
        if (!string.IsNullOrEmpty(profile.Description)) score += 7;
        if (!string.IsNullOrEmpty(profile.ShortDescription)) score += 6;
        if (profile.YearFounded.HasValue) score += 5;

        // Location (15%): city, province
        if (!string.IsNullOrEmpty(profile.City)) score += 8;
        if (!string.IsNullOrEmpty(profile.Province)) score += 7;

        // Contact (10%): website, phone, email — these live on Organization
        // but we check the profile's Organization nav prop or handle at caller.
        // For static calculation, we check the profile-level fields if available,
        // but the Organization fields are not always loaded. Use Organization nav if loaded.
        if (profile.Organization is not null)
        {
            if (!string.IsNullOrEmpty(profile.Organization.Website)) score += 4;
            if (!string.IsNullOrEmpty(profile.Organization.Phone)) score += 3;
            if (!string.IsNullOrEmpty(profile.Organization.Email)) score += 3;
        }

        // Sustainability (20%): any sustainability attribute set
        if (profile.RenewableEnergyPercent.HasValue) score += 5;
        if (profile.WasteRecyclingPercent.HasValue) score += 5;
        if (profile.CarbonReporting) score += 4;
        if (profile.WaterManagement) score += 3;
        if (profile.SustainablePackaging) score += 3;

        // Certifications (20%): at least 1
        if (certCount > 0) score += 20;

        // Logo (10%)
        if (!string.IsNullOrEmpty(profile.LogoUrl)) score += 10;

        return Math.Min(score, 100);
    }

    private async Task RunScoringAsync(SupplierProfile profile, CancellationToken ct)
    {
        var certs = await _context.SupplierCertifications
            .Where(c => c.SupplierProfileId == profile.Id)
            .ToListAsync(ct);

        var esgResult = _esgScoring.CalculateScore(profile, certs);
        profile.EsgLevel = esgResult.Level;
        profile.EsgScore = esgResult.NumericScore;

        var verificationStatus = _verification.Evaluate(profile, certs);
        profile.VerificationStatus = verificationStatus;

        profile.LastScoredAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    private async Task<SupplierProfileDto> BuildProfileDtoAsync(Guid profileId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .Include(p => p.Organization)
            .Include(p => p.SupplierIndustries).ThenInclude(si => si.Industry)
            .Include(p => p.SupplierServiceTags).ThenInclude(sst => sst.ServiceTag)
            .Include(p => p.Certifications).ThenInclude(c => c.CertificationType)
            .FirstAsync(p => p.Id == profileId, ct);

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
}
