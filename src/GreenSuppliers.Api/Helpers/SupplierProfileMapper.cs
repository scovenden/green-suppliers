using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Helpers;

/// <summary>
/// Shared SupplierProfile -> SupplierProfileDto mapping and DB-loading logic.
/// Extracted to eliminate duplicate BuildProfileDtoAsync and MapToDto between
/// SupplierService and SupplierMeService.
/// </summary>
public static class SupplierProfileMapper
{
    /// <summary>
    /// Loads a supplier profile by ID with all related data (org, industries, tags, certs)
    /// and maps it to a SupplierProfileDto. Used by both admin and self-service flows.
    /// </summary>
    public static async Task<SupplierProfileDto> BuildProfileDtoAsync(
        GreenSuppliersDbContext context, Guid profileId, CancellationToken ct)
    {
        var profile = await context.SupplierProfiles
            .AsNoTracking()
            .Include(p => p.Organization)
            .Include(p => p.SupplierIndustries).ThenInclude(si => si.Industry)
            .Include(p => p.SupplierServiceTags).ThenInclude(sst => sst.ServiceTag)
            .Include(p => p.Certifications).ThenInclude(c => c.CertificationType)
            .FirstAsync(p => p.Id == profileId, ct);

        return MapToDto(profile);
    }

    /// <summary>
    /// Pure mapping from a fully-loaded SupplierProfile entity to DTO.
    /// Requires Organization, SupplierIndustries.Industry, SupplierServiceTags.ServiceTag,
    /// and Certifications.CertificationType to be loaded.
    /// </summary>
    public static SupplierProfileDto MapToDto(SupplierProfile profile)
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
}
