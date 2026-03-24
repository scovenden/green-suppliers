using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Services;

public class VerificationService
{
    /// <summary>
    /// Evaluates the verification status for a supplier profile based on its
    /// current status and certifications. Pure function — no DB access.
    ///
    /// State machine:
    ///   unverified → pending    (cert uploaded/pending)
    ///   pending    → verified   (cert accepted + profile complete)
    ///   verified   → unverified (all certs expired, no valid certs remain)
    ///   any        → flagged    (admin action — NOT handled by this service)
    ///   flagged    → stays flagged (this service preserves flagged status)
    /// </summary>
    public VerificationStatus Evaluate(SupplierProfile profile, List<SupplierCertification> certs)
    {
        // If profile is currently Flagged, return Flagged (only admin can change this)
        if (profile.VerificationStatus == VerificationStatus.Flagged)
        {
            return VerificationStatus.Flagged;
        }

        // Filter valid certs: Status=Accepted AND (ExpiresAt null OR ExpiresAt > today)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var validCerts = certs.Where(c =>
            c.Status == CertificationStatus.Accepted &&
            (c.ExpiresAt is null || c.ExpiresAt > today))
            .ToList();

        // Check if any cert has Status=Pending
        var hasPendingCerts = certs.Any(c => c.Status == CertificationStatus.Pending);

        // Check profile completeness
        var isProfileComplete = IsProfileComplete(profile);

        // If at least 1 valid cert AND profile is complete → Verified
        if (validCerts.Count > 0 && isProfileComplete)
        {
            return VerificationStatus.Verified;
        }

        // If has pending certs OR (has accepted cert but profile incomplete) → Pending
        if (hasPendingCerts || (validCerts.Count > 0 && !isProfileComplete))
        {
            return VerificationStatus.Pending;
        }

        // Otherwise → Unverified
        return VerificationStatus.Unverified;
    }

    /// <summary>
    /// Profile completeness: TradingName, Description, CountryCode, City
    /// all present and non-empty.
    /// </summary>
    private static bool IsProfileComplete(SupplierProfile profile)
    {
        return !string.IsNullOrWhiteSpace(profile.TradingName) &&
               !string.IsNullOrWhiteSpace(profile.Description) &&
               !string.IsNullOrWhiteSpace(profile.CountryCode) &&
               !string.IsNullOrWhiteSpace(profile.City);
    }
}
