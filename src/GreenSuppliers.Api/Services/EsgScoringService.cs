using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Api.Services;

public record EsgScoreResult(EsgLevel Level, int NumericScore, List<string> Reasons);

public class EsgScoringService
{
    private static readonly string[] RequiredFieldNames =
    {
        nameof(SupplierProfile.TradingName),
        nameof(SupplierProfile.Description),
        nameof(SupplierProfile.CountryCode),
        nameof(SupplierProfile.City)
    };

    public EsgScoreResult CalculateScore(SupplierProfile profile, List<SupplierCertification> certs)
    {
        var reasons = new List<string>();

        // Check profile completeness
        var missingFields = GetMissingRequiredFields(profile);
        if (missingFields.Count > 0)
        {
            foreach (var field in missingFields)
            {
                reasons.Add($"Missing required field: {field}");
            }

            return new EsgScoreResult(EsgLevel.None, 0, reasons);
        }

        reasons.Add("Profile complete with all required fields");

        // Filter valid certifications: Accepted AND not expired
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var validCertCount = certs.Count(c =>
            c.Status == CertificationStatus.Accepted &&
            (c.ExpiresAt is null || c.ExpiresAt > today));

        if (validCertCount > 0)
        {
            reasons.Add($"{validCertCount} valid certification{(validCertCount == 1 ? "" : "s")}");
        }

        var renewablePercent = profile.RenewableEnergyPercent ?? 0;
        var wastePercent = profile.WasteRecyclingPercent ?? 0;

        if (renewablePercent > 0)
        {
            reasons.Add($"Renewable energy: {renewablePercent}%");
        }

        if (wastePercent > 0)
        {
            reasons.Add($"Waste recycling: {wastePercent}%");
        }

        if (profile.CarbonReporting)
        {
            reasons.Add("Carbon reporting: active");
        }

        // Evaluate from highest level down
        if (validCertCount >= 3 &&
            renewablePercent >= 70 &&
            wastePercent >= 70 &&
            profile.CarbonReporting)
        {
            return new EsgScoreResult(EsgLevel.Platinum, 100, reasons);
        }

        if (validCertCount >= 2 &&
            renewablePercent >= 50 &&
            profile.CarbonReporting)
        {
            return new EsgScoreResult(EsgLevel.Gold, 75, reasons);
        }

        if (validCertCount >= 1 &&
            renewablePercent >= 20)
        {
            return new EsgScoreResult(EsgLevel.Silver, 50, reasons);
        }

        return new EsgScoreResult(EsgLevel.Bronze, 25, reasons);
    }

    private static List<string> GetMissingRequiredFields(SupplierProfile profile)
    {
        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(profile.TradingName))
            missing.Add(nameof(SupplierProfile.TradingName));

        if (string.IsNullOrWhiteSpace(profile.Description))
            missing.Add(nameof(SupplierProfile.Description));

        if (string.IsNullOrWhiteSpace(profile.CountryCode))
            missing.Add(nameof(SupplierProfile.CountryCode));

        if (string.IsNullOrWhiteSpace(profile.City))
            missing.Add(nameof(SupplierProfile.City));

        return missing;
    }
}
