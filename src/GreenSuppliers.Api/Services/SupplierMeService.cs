using System.Text.Json;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSuppliers.Api.Services;

public class SupplierMeService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly EsgScoringService _esgScoring;
    private readonly VerificationService _verification;
    private readonly AuditService _audit;
    private readonly ILogger<SupplierMeService> _logger;

    public SupplierMeService(
        GreenSuppliersDbContext context,
        EsgScoringService esgScoring,
        VerificationService verification,
        AuditService audit,
        ILogger<SupplierMeService> logger)
    {
        _context = context;
        _esgScoring = esgScoring;
        _verification = verification;
        _audit = audit;
        _logger = logger;
    }

    /// <summary>
    /// Returns the supplier profile ID for an organization, or null if not found.
    /// Lightweight query — does not load related data.
    /// </summary>
    public async Task<Guid?> GetProfileIdByOrgAsync(Guid orgId, CancellationToken ct)
    {
        return await _context.SupplierProfiles
            .AsNoTracking()
            .Where(p => p.OrganizationId == orgId && !p.IsDeleted)
            .Select(p => (Guid?)p.Id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<SupplierProfileDto?> GetByOrganizationIdAsync(Guid orgId, CancellationToken ct)
    {
        var profileId = await GetProfileIdByOrgAsync(orgId, ct);

        if (profileId is null)
            return null;

        return await BuildProfileDtoAsync(profileId.Value, ct);
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

        // Validate that all submitted IndustryIds exist in the database
        if (request.IndustryIds.Count > 0)
        {
            var existingIndustryCount = await _context.Industries
                .CountAsync(i => request.IndustryIds.Contains(i.Id), ct);

            if (existingIndustryCount != request.IndustryIds.Count)
                throw new ArgumentException("One or more industry IDs are invalid.");
        }

        // Validate that all submitted ServiceTagIds exist in the database
        if (request.ServiceTagIds.Count > 0)
        {
            var existingTagCount = await _context.ServiceTags
                .CountAsync(t => request.ServiceTagIds.Contains(t.Id), ct);

            if (existingTagCount != request.ServiceTagIds.Count)
                throw new ArgumentException("One or more service tag IDs are invalid.");
        }

        // Capture old values for audit log
        var oldValues = JsonSerializer.Serialize(new
        {
            profile.TradingName,
            profile.Description,
            profile.ShortDescription,
            profile.YearFounded,
            profile.EmployeeCount,
            profile.BbbeeLevel,
            profile.City,
            profile.Province,
            profile.RenewableEnergyPercent,
            profile.WasteRecyclingPercent,
            profile.CarbonReporting,
            profile.WaterManagement,
            profile.SustainablePackaging,
            Website = profile.Organization.Website,
            Phone = profile.Organization.Phone,
            Email = profile.Organization.Email,
        });

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

        // Capture new values for audit log
        var newValues = JsonSerializer.Serialize(new
        {
            request.TradingName,
            request.Description,
            request.ShortDescription,
            request.YearFounded,
            request.EmployeeCount,
            request.BbbeeLevel,
            request.City,
            request.Province,
            request.RenewableEnergyPercent,
            request.WasteRecyclingPercent,
            request.CarbonReporting,
            request.WaterManagement,
            request.SustainablePackaging,
            request.Website,
            request.Phone,
            request.Email,
        });

        await _context.SaveChangesAsync(ct);

        // Re-run ESG scoring + verification after profile update
        await RunScoringAsync(profile, ct);

        // Write audit log with old + new values
        await _audit.LogAsync(userId, "SupplierSelfUpdated", "SupplierProfile", profile.Id,
            oldValues: oldValues, newValues: newValues, ct: ct);

        _logger.LogInformation(
            "Supplier profile updated. ProfileId={ProfileId} UserId={UserId} OrgId={OrgId}",
            profile.Id, userId, orgId);

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
        var certType = await _context.CertificationTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CertificationTypeId, ct);

        if (certType is null)
            throw new ArgumentException($"Certification type '{request.CertificationTypeId}' does not exist.");

        // If a DocumentId is provided, verify it belongs to this supplier's profile
        if (request.DocumentId.HasValue)
        {
            var documentBelongsToProfile = await _context.Documents
                .AnyAsync(d => d.Id == request.DocumentId.Value && d.SupplierProfileId == profile.Id, ct);

            if (!documentBelongsToProfile)
                throw new ArgumentException("The referenced document does not belong to this supplier profile.");
        }

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

        _logger.LogInformation(
            "Certification submitted. CertificationId={CertificationId} ProfileId={ProfileId} UserId={UserId}",
            certification.Id, profile.Id, userId);

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

    /// <summary>
    /// Returns a result object indicating success/failure with a reason code.
    /// </summary>
    public async Task<(bool Success, string? FailureReason)> RequestPublicationAsync(Guid orgId, Guid userId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .Include(p => p.Organization)
            .Include(p => p.Certifications)
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted, ct);

        if (profile is null)
            return (false, "PROFILE_NOT_FOUND");

        // Flagged profiles cannot be published by the supplier
        if (profile.VerificationStatus == VerificationStatus.Flagged)
            return (false, "PROFILE_FLAGGED");

        // Check profile completeness — must be above a threshold to publish
        var certCount = profile.Certifications.Count;
        var completeness = CalculateCompleteness(profile, certCount);

        // Require at least 50% completeness to publish
        if (completeness < 50)
            return (false, "INCOMPLETE_PROFILE");

        profile.IsPublished = true;
        profile.PublishedAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        // Audit log — publication is a significant state change
        await _audit.LogAsync(userId, "ProfilePublished", "SupplierProfile", profile.Id, ct: ct);

        _logger.LogInformation(
            "Supplier profile published. ProfileId={ProfileId} UserId={UserId} OrgId={OrgId} Completeness={Completeness}",
            profile.Id, userId, orgId, completeness);

        return (true, null);
    }

    public async Task<SupplierDashboardDto> GetDashboardStatsAsync(Guid orgId, CancellationToken ct)
    {
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .Include(p => p.Organization)
            .FirstOrDefaultAsync(p => p.OrganizationId == orgId && !p.IsDeleted, ct);

        if (profile is null)
        {
            return new SupplierDashboardDto();
        }

        // Use grouped counts in fewer round-trips
        var leadCounts = await _context.Leads
            .Where(l => l.SupplierProfileId == profile.Id)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                New = g.Count(l => l.Status == LeadStatus.New)
            })
            .FirstOrDefaultAsync(ct);

        var certCounts = await _context.SupplierCertifications
            .Where(c => c.SupplierProfileId == profile.Id)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Pending = g.Count(c => c.Status == CertificationStatus.Pending)
            })
            .FirstOrDefaultAsync(ct);

        var completeness = CalculateCompleteness(profile, certCounts?.Total ?? 0);

        return new SupplierDashboardDto
        {
            LeadCount = leadCounts?.Total ?? 0,
            NewLeadCount = leadCounts?.New ?? 0,
            CertificationCount = certCounts?.Total ?? 0,
            PendingCertCount = certCounts?.Pending ?? 0,
            EsgLevel = profile.EsgLevel.ToString(),
            EsgScore = profile.EsgScore,
            VerificationStatus = profile.VerificationStatus.ToString(),
            IsPublished = profile.IsPublished,
            ProfileCompleteness = completeness
        };
    }

    /// <summary>
    /// Returns certifications for a given profile, with lightweight query (no full profile load).
    /// </summary>
    public async Task<List<SupplierCertificationDto>> GetCertificationsByOrgAsync(Guid orgId, CancellationToken ct)
    {
        return await _context.SupplierCertifications
            .AsNoTracking()
            .Where(c => c.SupplierProfile.OrganizationId == orgId && !c.SupplierProfile.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new SupplierCertificationDto
            {
                Id = c.Id,
                CertificationTypeName = c.CertificationType.Name,
                CertificateNumber = c.CertificateNumber,
                IssuedAt = c.IssuedAt,
                ExpiresAt = c.ExpiresAt,
                Status = c.Status.ToString()
            })
            .ToListAsync(ct);
    }

    public async Task<PagedResult<LeadDto>> GetLeadsAsync(Guid orgId, int page, int pageSize, string? status, CancellationToken ct)
    {
        var profileId = await GetProfileIdByOrgAsync(orgId, ct);

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
            Items = items.Select(MapLeadToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<LeadDto?> GetLeadDetailAsync(Guid orgId, Guid leadId, CancellationToken ct)
    {
        var profileId = await GetProfileIdByOrgAsync(orgId, ct);

        if (profileId is null)
            return null;

        var lead = await _context.Leads
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leadId && l.SupplierProfileId == profileId.Value, ct);

        if (lead is null)
            return null;

        return MapLeadToDto(lead);
    }

    public async Task<bool> UpdateLeadStatusAsync(Guid orgId, Guid leadId, string newStatus, Guid userId, CancellationToken ct)
    {
        if (!Enum.TryParse<LeadStatus>(newStatus, ignoreCase: true, out var parsedStatus))
            return false;

        var profileId = await GetProfileIdByOrgAsync(orgId, ct);

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
