using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class SdgService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly AuditService _audit;

    public SdgService(GreenSuppliersDbContext context, AuditService audit)
    {
        _context = context;
        _audit = audit;
    }

    /// <summary>
    /// Returns all 17 UN SDGs, ordered by ID (1-17).
    /// </summary>
    public async Task<List<SdgDto>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Sdgs
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new SdgDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                Color = s.Color
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// Updates the SDGs linked to a supplier profile.
    /// Validates that all SDG IDs are in the range 1-17.
    /// </summary>
    public async Task<List<SdgDto>> UpdateSupplierSdgsAsync(
        Guid profileId, List<int> sdgIds, Guid userId, CancellationToken ct)
    {
        // Validate all SDG IDs are in valid range
        var invalidIds = sdgIds.Where(id => id < 1 || id > 17).ToList();
        if (invalidIds.Count > 0)
            throw new ArgumentException($"Invalid SDG IDs: {string.Join(", ", invalidIds)}. Valid range is 1-17.");

        // Verify the profile exists
        var profileExists = await _context.SupplierProfiles
            .AnyAsync(p => p.Id == profileId && !p.IsDeleted, ct);

        if (!profileExists)
            throw new KeyNotFoundException("Supplier profile not found.");

        // Verify all SDG IDs exist in the database
        var uniqueIds = sdgIds.Distinct().ToList();
        var existingCount = await _context.Sdgs.CountAsync(s => uniqueIds.Contains(s.Id), ct);
        if (existingCount != uniqueIds.Count)
            throw new ArgumentException("One or more SDG IDs do not exist.");

        // Remove existing links
        var existingLinks = await _context.Set<SupplierSdg>()
            .Where(ss => ss.SupplierProfileId == profileId)
            .ToListAsync(ct);
        _context.Set<SupplierSdg>().RemoveRange(existingLinks);

        // Add new links
        foreach (var sdgId in uniqueIds)
        {
            _context.Set<SupplierSdg>().Add(new SupplierSdg
            {
                SupplierProfileId = profileId,
                SdgId = sdgId
            });
        }

        await _context.SaveChangesAsync(ct);

        // Audit log
        await _audit.LogAsync(userId, "SdgsUpdated", "SupplierProfile", profileId,
            newValues: $"{{\"sdgIds\":[{string.Join(",", uniqueIds)}]}}", ct: ct);

        // Return the updated SDG list
        return await _context.Set<SupplierSdg>()
            .AsNoTracking()
            .Where(ss => ss.SupplierProfileId == profileId)
            .OrderBy(ss => ss.SdgId)
            .Select(ss => new SdgDto
            {
                Id = ss.Sdg.Id,
                Name = ss.Sdg.Name,
                Description = ss.Sdg.Description,
                Color = ss.Sdg.Color
            })
            .ToListAsync(ct);
    }
}
