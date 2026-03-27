using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class PlacementService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly AuditService _audit;

    public PlacementService(GreenSuppliersDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    /// <summary>
    /// Returns currently active sponsored placements (within date range and IsActive=true).
    /// Used by the public-facing featured suppliers endpoint.
    /// </summary>
    public async Task<List<PlacementDto>> GetFeaturedAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        return await _context.SponsoredPlacements
            .AsNoTracking()
            .Include(sp => sp.SupplierProfile)
                .ThenInclude(p => p.Organization)
            .Where(sp => sp.IsActive && sp.StartsAt <= now && sp.EndsAt >= now
                && sp.SupplierProfile.IsPublished && !sp.SupplierProfile.IsDeleted)
            .OrderByDescending(sp => sp.CreatedAt)
            .Select(sp => MapToDto(sp))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Returns all placements for admin (paginated).
    /// </summary>
    public async Task<PagedResult<PlacementDto>> GetAllAsync(int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.SponsoredPlacements
            .AsNoTracking()
            .Include(sp => sp.SupplierProfile)
                .ThenInclude(p => p.Organization)
            .OrderByDescending(sp => sp.CreatedAt);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(sp => MapToDto(sp))
            .ToListAsync(ct);

        return new PagedResult<PlacementDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    /// <summary>
    /// Creates a new sponsored placement. Admin only.
    /// </summary>
    public async Task<PlacementDto> CreateAsync(CreatePlacementRequest request, Guid adminUserId, CancellationToken ct)
    {
        // Verify the supplier profile exists
        var profile = await _context.SupplierProfiles
            .AsNoTracking()
            .Include(p => p.Organization)
            .FirstOrDefaultAsync(p => p.Id == request.SupplierProfileId && !p.IsDeleted, ct);

        if (profile is null)
            throw new KeyNotFoundException("Supplier profile not found.");

        if (request.EndsAt <= request.StartsAt)
            throw new ArgumentException("End date must be after start date.");

        var placement = new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = request.SupplierProfileId,
            PlacementType = request.PlacementType,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            IsActive = true,
            ImpressionsCount = 0,
            ClicksCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.SponsoredPlacements.Add(placement);
        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(adminUserId, "PlacementCreated", "SponsoredPlacement", placement.Id, ct: ct);

        return new PlacementDto
        {
            Id = placement.Id,
            SupplierProfileId = placement.SupplierProfileId,
            SupplierName = profile.Organization.Name,
            SupplierSlug = profile.Slug,
            LogoUrl = profile.LogoUrl,
            PlacementType = placement.PlacementType,
            StartsAt = placement.StartsAt,
            EndsAt = placement.EndsAt,
            ImpressionsCount = placement.ImpressionsCount,
            ClicksCount = placement.ClicksCount,
            IsActive = placement.IsActive,
            CreatedAt = placement.CreatedAt
        };
    }

    /// <summary>
    /// Updates an existing placement (partial update). Admin only.
    /// </summary>
    public async Task<PlacementDto?> UpdateAsync(Guid id, UpdatePlacementRequest request, Guid adminUserId, CancellationToken ct)
    {
        var placement = await _context.SponsoredPlacements
            .Include(sp => sp.SupplierProfile)
                .ThenInclude(p => p.Organization)
            .FirstOrDefaultAsync(sp => sp.Id == id, ct);

        if (placement is null)
            return null;

        if (request.PlacementType is not null)
            placement.PlacementType = request.PlacementType;

        if (request.StartsAt.HasValue)
            placement.StartsAt = request.StartsAt.Value;

        if (request.EndsAt.HasValue)
            placement.EndsAt = request.EndsAt.Value;

        if (request.IsActive.HasValue)
            placement.IsActive = request.IsActive.Value;

        // Validate date range after applying changes
        if (placement.EndsAt <= placement.StartsAt)
            throw new ArgumentException("End date must be after start date.");

        await _context.SaveChangesAsync(ct);

        await _audit.LogAsync(adminUserId, "PlacementUpdated", "SponsoredPlacement", placement.Id, ct: ct);

        return MapToDto(placement);
    }

    /// <summary>
    /// Records an impression for a placement.
    /// </summary>
    public async Task RecordImpressionAsync(Guid placementId, CancellationToken ct)
    {
        var placement = await _context.SponsoredPlacements
            .FirstOrDefaultAsync(sp => sp.Id == placementId, ct);

        if (placement is not null)
        {
            placement.ImpressionsCount++;
            await _context.SaveChangesAsync(ct);
        }
    }

    /// <summary>
    /// Records a click for a placement.
    /// </summary>
    public async Task RecordClickAsync(Guid placementId, CancellationToken ct)
    {
        var placement = await _context.SponsoredPlacements
            .FirstOrDefaultAsync(sp => sp.Id == placementId, ct);

        if (placement is not null)
        {
            placement.ClicksCount++;
            await _context.SaveChangesAsync(ct);
        }
    }

    private static PlacementDto MapToDto(SponsoredPlacement sp) => new()
    {
        Id = sp.Id,
        SupplierProfileId = sp.SupplierProfileId,
        SupplierName = sp.SupplierProfile.Organization.Name,
        SupplierSlug = sp.SupplierProfile.Slug,
        LogoUrl = sp.SupplierProfile.LogoUrl,
        PlacementType = sp.PlacementType,
        StartsAt = sp.StartsAt,
        EndsAt = sp.EndsAt,
        ImpressionsCount = sp.ImpressionsCount,
        ClicksCount = sp.ClicksCount,
        IsActive = sp.IsActive,
        CreatedAt = sp.CreatedAt
    };
}
