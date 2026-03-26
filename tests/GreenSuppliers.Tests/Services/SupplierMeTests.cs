using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreenSuppliers.Tests.Services;

public class SupplierMeTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static SupplierMeService CreateService(GreenSuppliersDbContext context)
    {
        var esgScoring = new EsgScoringService();
        var verification = new VerificationService();
        var audit = new AuditService(context);
        var logger = Mock.Of<ILogger<SupplierMeService>>();
        return new SupplierMeService(context, esgScoring, verification, audit, logger);
    }

    private static async Task<(Guid OrgId, Guid ProfileId)> SeedSupplierAsync(
        GreenSuppliersDbContext context,
        Action<SupplierProfile>? configureProfile = null,
        Action<Organization>? configureOrg = null)
    {
        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Eco Solutions (Pty) Ltd",
            CountryCode = "ZA",
            City = "Cape Town",
            Province = "Western Cape",
            Website = "https://ecosolutions.co.za",
            Phone = "+27211234567",
            Email = "info@ecosolutions.co.za",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = now,
            UpdatedAt = now
        };
        configureOrg?.Invoke(org);
        context.Organizations.Add(org);

        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Slug = "eco-solutions",
            TradingName = "Eco Solutions",
            Description = "A leading provider of sustainable packaging solutions.",
            ShortDescription = "Sustainable packaging",
            CountryCode = "ZA",
            City = "Cape Town",
            Province = "Western Cape",
            RenewableEnergyPercent = 30,
            WasteRecyclingPercent = 40,
            CarbonReporting = true,
            WaterManagement = false,
            SustainablePackaging = true,
            IsPublished = true,
            PublishedAt = now,
            EsgLevel = EsgLevel.Bronze,
            EsgScore = 25,
            VerificationStatus = VerificationStatus.Unverified,
            CreatedAt = now,
            UpdatedAt = now
        };
        configureProfile?.Invoke(profile);
        context.SupplierProfiles.Add(profile);

        await context.SaveChangesAsync();
        return (org.Id, profile.Id);
    }

    [Fact]
    public async Task GetByOrgId_ReturnsProfile()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        // Act
        var result = await service.GetByOrganizationIdAsync(orgId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profileId);
        result.OrganizationId.Should().Be(orgId);
        result.TradingName.Should().Be("Eco Solutions");
    }

    [Fact]
    public async Task GetByOrgId_WrongOrg_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        await SeedSupplierAsync(context);

        // Act
        var result = await service.GetByOrganizationIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateOwnProfile_UpdatesEditableFields()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        var request = new UpdateMyProfileRequest
        {
            TradingName = "Eco Solutions Updated",
            Description = "Updated description for sustainable solutions.",
            ShortDescription = "Updated short desc",
            YearFounded = 2015,
            EmployeeCount = "50-100",
            BbbeeLevel = "Level 2",
            City = "Johannesburg",
            Province = "Gauteng",
            Website = "https://updated.co.za",
            Phone = "+27111234567",
            Email = "updated@ecosolutions.co.za",
            RenewableEnergyPercent = 60,
            WasteRecyclingPercent = 50,
            CarbonReporting = true,
            WaterManagement = true,
            SustainablePackaging = false
        };

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TradingName.Should().Be("Eco Solutions Updated");
        result.Description.Should().Be("Updated description for sustainable solutions.");
        result.ShortDescription.Should().Be("Updated short desc");
        result.YearFounded.Should().Be(2015);
        result.EmployeeCount.Should().Be("50-100");
        result.BbbeeLevel.Should().Be("Level 2");
        result.City.Should().Be("Johannesburg");
        result.Province.Should().Be("Gauteng");
        result.Website.Should().Be("https://updated.co.za");
        result.Phone.Should().Be("+27111234567");
        result.Email.Should().Be("updated@ecosolutions.co.za");
        result.RenewableEnergyPercent.Should().Be(60);
        result.CarbonReporting.Should().BeTrue();
        result.WaterManagement.Should().BeTrue();
        result.SustainablePackaging.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOwnProfile_CannotChangeVerificationStatus()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context, profile =>
        {
            profile.VerificationStatus = VerificationStatus.Flagged;
        });
        var userId = Guid.NewGuid();

        var request = new UpdateMyProfileRequest
        {
            TradingName = "Updated Name",
            Description = "Updated desc",
            City = "Cape Town"
        };

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert — UpdateMyProfileRequest has no VerificationStatus field,
        // and Flagged profiles stay Flagged after re-scoring (VerificationService preserves it).
        result.Should().NotBeNull();
        result!.VerificationStatus.Should().Be(VerificationStatus.Flagged);
    }

    [Fact]
    public async Task UpdateOwnProfile_TriggersRescore()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context, profile =>
        {
            // Start with incomplete profile (no TradingName) so ESG = None
            profile.TradingName = null;
            profile.EsgLevel = EsgLevel.None;
            profile.EsgScore = 0;
        });
        var userId = Guid.NewGuid();

        var request = new UpdateMyProfileRequest
        {
            TradingName = "Eco Solutions Complete",
            Description = "Full description provided.",
            City = "Cape Town",
            RenewableEnergyPercent = 30
        };

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert — profile is now complete, should be at least Bronze
        result.Should().NotBeNull();
        result!.EsgLevel.Should().NotBe(EsgLevel.None);
        result.EsgScore.Should().BeGreaterThan(0);
        result.LastScoredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task AddCertification_SetsStatusToPending()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        var certType = new CertificationType
        {
            Id = Guid.NewGuid(),
            Name = "ISO 14001",
            Slug = "iso-14001",
            CreatedAt = DateTime.UtcNow
        };
        context.CertificationTypes.Add(certType);
        await context.SaveChangesAsync();

        var request = new AddCertificationRequest
        {
            CertificationTypeId = certType.Id,
            CertificateNumber = "CERT-2026-001",
            IssuedAt = new DateOnly(2025, 1, 1),
            ExpiresAt = new DateOnly(2028, 1, 1)
        };

        // Act
        var result = await service.AddCertificationAsync(orgId, request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Pending");
        result.CertTypeName.Should().Be("ISO 14001");
        result.CertificateNumber.Should().Be("CERT-2026-001");

        // Verify it was saved to DB
        var dbCert = await context.SupplierCertifications.FirstOrDefaultAsync();
        dbCert.Should().NotBeNull();
        dbCert!.Status.Should().Be(CertificationStatus.Pending);
    }

    [Fact]
    public async Task RequestPublication_SetsPublished()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _) = await SeedSupplierAsync(context, profile =>
        {
            // Profile is complete enough (trading name, description, city, province, etc.)
            profile.IsPublished = false;
            profile.PublishedAt = null;
            profile.YearFounded = 2015;
        });

        // Act
        var (success, _) = await service.RequestPublicationAsync(orgId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        var profile = await context.SupplierProfiles
            .FirstAsync(p => p.OrganizationId == orgId);
        profile.IsPublished.Should().BeTrue();
        profile.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RequestPublication_IncompleteProfile_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _) = await SeedSupplierAsync(context, profile =>
        {
            // Strip most fields to make completeness < 50%
            profile.TradingName = null;
            profile.Description = null;
            profile.ShortDescription = null;
            profile.City = null;
            profile.Province = null;
            profile.YearFounded = null;
            profile.RenewableEnergyPercent = null;
            profile.WasteRecyclingPercent = null;
            profile.CarbonReporting = false;
            profile.WaterManagement = false;
            profile.SustainablePackaging = false;
            profile.LogoUrl = null;
            profile.IsPublished = false;
            profile.PublishedAt = null;
        }, org =>
        {
            org.Website = null;
            org.Phone = null;
            org.Email = null;
        });

        // Act
        var (success, _) = await service.RequestPublicationAsync(orgId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        success.Should().BeFalse();
    }

    [Fact]
    public async Task GetDashboardStats_ReturnsCorrectCounts()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        // Add leads
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            ContactName = "John Doe",
            ContactEmail = "john@example.com",
            Message = "Interested in your services",
            Status = LeadStatus.New,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            ContactName = "Jane Doe",
            ContactEmail = "jane@example.com",
            Message = "Need a quote",
            Status = LeadStatus.Contacted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Add certifications
        var certType = new CertificationType
        {
            Id = Guid.NewGuid(),
            Name = "ISO 14001",
            Slug = "iso-14001",
            CreatedAt = DateTime.UtcNow
        };
        context.CertificationTypes.Add(certType);
        context.SupplierCertifications.Add(new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            CertificationTypeId = certType.Id,
            Status = CertificationStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.SupplierCertifications.Add(new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            CertificationTypeId = certType.Id,
            Status = CertificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDashboardStatsAsync(orgId, CancellationToken.None);

        // Assert
        result.LeadCount.Should().Be(2);
        result.NewLeadCount.Should().Be(1);
        result.CertificationCount.Should().Be(2);
        result.PendingCertCount.Should().Be(1);
        result.IsPublished.Should().BeTrue();
        result.EsgLevel.Should().NotBeNullOrEmpty();
        result.ProfileCompleteness.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ProfileCompleteness_EmptyProfile_Returns0()
    {
        // Arrange
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Slug = "empty",
            CountryCode = "ZA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var completeness = SupplierMeService.CalculateCompleteness(profile, 0);

        // Assert
        completeness.Should().Be(0);
    }

    [Fact]
    public void ProfileCompleteness_FullProfile_Returns100()
    {
        // Arrange
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Full Profile Corp",
            CountryCode = "ZA",
            Website = "https://full.co.za",
            Phone = "+27211234567",
            Email = "info@full.co.za",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Organization = org,
            Slug = "full-profile",
            TradingName = "Full Profile Corp",
            Description = "A complete description of the company.",
            ShortDescription = "Short desc",
            YearFounded = 2010,
            City = "Johannesburg",
            Province = "Gauteng",
            CountryCode = "ZA",
            RenewableEnergyPercent = 50,
            WasteRecyclingPercent = 60,
            CarbonReporting = true,
            WaterManagement = true,
            SustainablePackaging = true,
            LogoUrl = "https://example.com/logo.png",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act — certCount = 1 to satisfy the certification bucket
        var completeness = SupplierMeService.CalculateCompleteness(profile, 1);

        // Assert
        completeness.Should().Be(100);
    }
}
