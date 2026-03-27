using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class SqlFullTextSearchService : ISupplierSearchService
{
    private const int MaxPageSize = 50;

    private readonly GreenSuppliersDbContext _context;

    public SqlFullTextSearchService(GreenSuppliersDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SupplierSearchResult>> SearchAsync(SupplierSearchQuery searchQuery, CancellationToken ct = default)
    {
        // Cap page size
        var pageSize = Math.Min(Math.Max(searchQuery.PageSize, 1), MaxPageSize);
        var page = Math.Max(searchQuery.Page, 1);

        // Base query: published, non-deleted supplier profiles
        var query = _context.SupplierProfiles
            .Include(sp => sp.SupplierIndustries)
                .ThenInclude(si => si.Industry)
            .Include(sp => sp.SupplierServiceTags)
                .ThenInclude(sst => sst.ServiceTag)
            .Include(sp => sp.Certifications)
                .ThenInclude(c => c.CertificationType)
            .Include(sp => sp.SupplierSdgs)
            .Where(sp => sp.IsPublished && !sp.IsDeleted)
            .AsQueryable();

        // Filter 1: Text search (simple Contains for InMemory DB compatibility)
        if (!string.IsNullOrWhiteSpace(searchQuery.Q))
        {
            var q = searchQuery.Q;
            query = query.Where(sp =>
                (sp.TradingName != null && sp.TradingName.Contains(q)) ||
                (sp.Description != null && sp.Description.Contains(q)) ||
                (sp.ShortDescription != null && sp.ShortDescription.Contains(q)));
        }

        // Filter 2: Country code
        if (!string.IsNullOrWhiteSpace(searchQuery.CountryCode))
        {
            query = query.Where(sp => sp.CountryCode == searchQuery.CountryCode);
        }

        // Filter 3: Industry slug
        if (!string.IsNullOrWhiteSpace(searchQuery.IndustrySlug))
        {
            query = query.Where(sp =>
                sp.SupplierIndustries.Any(si => si.Industry.Slug == searchQuery.IndustrySlug));
        }

        // Filter 4: ESG level
        if (!string.IsNullOrWhiteSpace(searchQuery.EsgLevel) &&
            Enum.TryParse<EsgLevel>(searchQuery.EsgLevel, ignoreCase: true, out var esgLevel))
        {
            query = query.Where(sp => sp.EsgLevel == esgLevel);
        }

        // Filter 5: Verification status
        if (!string.IsNullOrWhiteSpace(searchQuery.VerificationStatus) &&
            Enum.TryParse<VerificationStatus>(searchQuery.VerificationStatus, ignoreCase: true, out var verStatus))
        {
            query = query.Where(sp => sp.VerificationStatus == verStatus);
        }

        // Filter 6: Certification type slug (must be Accepted)
        if (!string.IsNullOrWhiteSpace(searchQuery.CertTypeSlug))
        {
            query = query.Where(sp =>
                sp.Certifications.Any(c =>
                    c.CertificationType.Slug == searchQuery.CertTypeSlug &&
                    c.Status == CertificationStatus.Accepted));
        }

        // Filter 7: Tags (comma-separated slugs)
        if (!string.IsNullOrWhiteSpace(searchQuery.Tags))
        {
            var tagSlugs = searchQuery.Tags
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

            if (tagSlugs.Count > 0)
            {
                query = query.Where(sp =>
                    sp.SupplierServiceTags.Any(sst => tagSlugs.Contains(sst.ServiceTag.Slug)));
            }
        }

        // Filter 8: SDG
        if (searchQuery.Sdg.HasValue)
        {
            var sdgId = searchQuery.Sdg.Value;
            query = query.Where(sp =>
                sp.SupplierSdgs.Any(ss => ss.SdgId == sdgId));
        }

        // Get total count before pagination
        var total = await query.CountAsync(ct);

        // Sorting
        query = searchQuery.SortBy?.ToLowerInvariant() switch
        {
            "name" => query.OrderBy(sp => sp.TradingName),
            "newest" => query.OrderByDescending(sp => sp.CreatedAt),
            _ => query.OrderByDescending(sp => sp.EsgScore) // default: esgScore
        };

        // Pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sp => new SupplierSearchResult
            {
                Id = sp.Id,
                Slug = sp.Slug,
                TradingName = sp.TradingName ?? string.Empty,
                ShortDescription = sp.ShortDescription,
                City = sp.City,
                CountryCode = sp.CountryCode,
                VerificationStatus = sp.VerificationStatus.ToString(),
                EsgLevel = sp.EsgLevel.ToString(),
                EsgScore = sp.EsgScore,
                LogoUrl = sp.LogoUrl,
                Industries = sp.SupplierIndustries
                    .Select(si => si.Industry.Name)
                    .ToList(),
                SdgIds = sp.SupplierSdgs
                    .Select(ss => ss.SdgId)
                    .ToList(),
                IsVerified = sp.VerificationStatus == VerificationStatus.Verified
            })
            .ToListAsync(ct);

        return new PagedResult<SupplierSearchResult>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }
}
