using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

/// <summary>
/// Shared scoring logic that loads certifications, runs ESG scoring and verification
/// evaluation, and persists the results. Extracted from SupplierService and SupplierMeService
/// which had identical RunScoringAsync implementations.
/// </summary>
public class ScoringRunner
{
    private readonly GreenSuppliersDbContext _context;
    private readonly EsgScoringService _esgScoring;
    private readonly VerificationService _verification;

    public ScoringRunner(
        GreenSuppliersDbContext context,
        EsgScoringService esgScoring,
        VerificationService verification)
    {
        _context = context;
        _esgScoring = esgScoring;
        _verification = verification;
    }

    /// <summary>
    /// Loads all certifications for the given profile, recalculates ESG level/score
    /// and verification status, updates the profile, and saves changes.
    /// The profile entity must be tracked (not AsNoTracking).
    /// </summary>
    public async Task RunScoringAsync(SupplierProfile profile, CancellationToken ct)
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
}
