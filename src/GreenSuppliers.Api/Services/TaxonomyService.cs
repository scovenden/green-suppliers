using System.Text.RegularExpressions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class TaxonomyService
{
    private readonly GreenSuppliersDbContext _context;

    public TaxonomyService(GreenSuppliersDbContext context)
    {
        _context = context;
    }

    // --- Industries ---

    public async Task<List<IndustryDto>> GetIndustriesAsync(bool activeOnly = true)
    {
        var query = _context.Industries.AsNoTracking().AsQueryable();

        if (activeOnly)
            query = query.Where(i => i.IsActive);

        var industries = await query
            .OrderBy(i => i.SortOrder)
            .ThenBy(i => i.Name)
            .ToListAsync();

        // Get supplier counts
        var supplierCounts = await _context.Set<SupplierIndustry>()
            .GroupBy(si => si.IndustryId)
            .Select(g => new { IndustryId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.IndustryId, x => x.Count);

        return industries.Select(i => new IndustryDto
        {
            Id = i.Id,
            Name = i.Name,
            Slug = i.Slug,
            Description = i.Description,
            ParentId = i.ParentId,
            SortOrder = i.SortOrder,
            IsActive = i.IsActive,
            SupplierCount = supplierCounts.GetValueOrDefault(i.Id, 0)
        }).ToList();
    }

    public async Task<IndustryDto?> GetIndustryBySlugAsync(string slug)
    {
        var industry = await _context.Industries
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Slug == slug);

        if (industry is null) return null;

        var supplierCount = await _context.Set<SupplierIndustry>()
            .CountAsync(si => si.IndustryId == industry.Id);

        return new IndustryDto
        {
            Id = industry.Id,
            Name = industry.Name,
            Slug = industry.Slug,
            Description = industry.Description,
            ParentId = industry.ParentId,
            SortOrder = industry.SortOrder,
            IsActive = industry.IsActive,
            SupplierCount = supplierCount
        };
    }

    public async Task<IndustryDto> CreateIndustryAsync(string name, string? description, Guid? parentId)
    {
        var slug = Slugify(name);
        var now = DateTime.UtcNow;

        var industry = new Industry
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            Description = description,
            ParentId = parentId,
            SortOrder = 0,
            IsActive = true,
            CreatedAt = now
        };

        _context.Industries.Add(industry);
        await _context.SaveChangesAsync();

        return new IndustryDto
        {
            Id = industry.Id,
            Name = industry.Name,
            Slug = industry.Slug,
            Description = industry.Description,
            ParentId = industry.ParentId,
            SortOrder = industry.SortOrder,
            IsActive = industry.IsActive,
            SupplierCount = 0
        };
    }

    public async Task<IndustryDto?> UpdateIndustryAsync(Guid id, string name, string? description)
    {
        var industry = await _context.Industries.FindAsync(id);
        if (industry is null) return null;

        industry.Name = name;
        industry.Slug = Slugify(name);
        industry.Description = description;

        await _context.SaveChangesAsync();

        var supplierCount = await _context.Set<SupplierIndustry>()
            .CountAsync(si => si.IndustryId == id);

        return new IndustryDto
        {
            Id = industry.Id,
            Name = industry.Name,
            Slug = industry.Slug,
            Description = industry.Description,
            ParentId = industry.ParentId,
            SortOrder = industry.SortOrder,
            IsActive = industry.IsActive,
            SupplierCount = supplierCount
        };
    }

    // --- Certification Types ---

    public async Task<List<CertTypeDto>> GetCertTypesAsync(bool activeOnly = true)
    {
        var query = _context.CertificationTypes.AsNoTracking().AsQueryable();

        if (activeOnly)
            query = query.Where(ct => ct.IsActive);

        var certTypes = await query
            .OrderBy(ct => ct.Name)
            .ToListAsync();

        return certTypes.Select(ct => new CertTypeDto
        {
            Id = ct.Id,
            Name = ct.Name,
            Slug = ct.Slug,
            Description = ct.Description,
            IsActive = ct.IsActive
        }).ToList();
    }

    public async Task<CertTypeDto> CreateCertTypeAsync(string name, string? description)
    {
        var certType = new CertificationType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = Slugify(name),
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.CertificationTypes.Add(certType);
        await _context.SaveChangesAsync();

        return new CertTypeDto
        {
            Id = certType.Id,
            Name = certType.Name,
            Slug = certType.Slug,
            Description = certType.Description,
            IsActive = certType.IsActive
        };
    }

    public async Task<CertTypeDto?> UpdateCertTypeAsync(Guid id, string name, string? description)
    {
        var certType = await _context.CertificationTypes.FindAsync(id);
        if (certType is null) return null;

        certType.Name = name;
        certType.Slug = Slugify(name);
        certType.Description = description;

        await _context.SaveChangesAsync();

        return new CertTypeDto
        {
            Id = certType.Id,
            Name = certType.Name,
            Slug = certType.Slug,
            Description = certType.Description,
            IsActive = certType.IsActive
        };
    }

    // --- Service Tags ---

    public async Task<List<ServiceTagDto>> GetServiceTagsAsync(bool activeOnly = true)
    {
        var query = _context.ServiceTags.AsNoTracking().AsQueryable();

        if (activeOnly)
            query = query.Where(st => st.IsActive);

        var tags = await query
            .OrderBy(st => st.Name)
            .ToListAsync();

        return tags.Select(st => new ServiceTagDto
        {
            Id = st.Id,
            Name = st.Name,
            Slug = st.Slug,
            IsActive = st.IsActive
        }).ToList();
    }

    public async Task<ServiceTagDto> CreateServiceTagAsync(string name)
    {
        var tag = new ServiceTag
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = Slugify(name),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ServiceTags.Add(tag);
        await _context.SaveChangesAsync();

        return new ServiceTagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Slug = tag.Slug,
            IsActive = tag.IsActive
        };
    }

    // --- Countries ---

    public async Task<List<CountryDto>> GetCountriesAsync(bool activeOnly = true)
    {
        var query = _context.Countries.AsNoTracking().AsQueryable();

        if (activeOnly)
            query = query.Where(c => c.IsActive);

        var countries = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // Get supplier counts by country code
        var supplierCounts = await _context.SupplierProfiles
            .Where(sp => !sp.IsDeleted)
            .GroupBy(sp => sp.CountryCode)
            .Select(g => new { CountryCode = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CountryCode, x => x.Count);

        return countries.Select(c => new CountryDto
        {
            Code = c.Code,
            Name = c.Name,
            Slug = c.Slug,
            Region = c.Region,
            IsActive = c.IsActive,
            SupplierCount = supplierCounts.GetValueOrDefault(c.Code, 0)
        }).ToList();
    }

    public async Task<CountryDto?> GetCountryByCodeAsync(string code)
    {
        var country = await _context.Countries
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Code == code);

        if (country is null) return null;

        var supplierCount = await _context.SupplierProfiles
            .CountAsync(sp => sp.CountryCode == code && !sp.IsDeleted);

        return new CountryDto
        {
            Code = country.Code,
            Name = country.Name,
            Slug = country.Slug,
            Region = country.Region,
            IsActive = country.IsActive,
            SupplierCount = supplierCount
        };
    }

    private static string Slugify(string input)
    {
        var slug = input.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[\s_]+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = Regex.Replace(slug, @"-{2,}", "-");
        slug = slug.Trim('-');
        return slug;
    }
}
