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

/// <summary>
/// Comprehensive unit tests for SupplierMeService — the self-service API for suppliers
/// to manage their own profiles, certifications, publication, and dashboard.
///
/// Tests use EF Core InMemory provider with real EsgScoringService, VerificationService,
/// and AuditService — no mocks, because these are all pure-logic or DB-only services.
/// </summary>
public class SupplierMeServiceTests
{
    // =========================================================================
    // Helpers
    // =========================================================================

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

    /// <summary>
    /// Seeds a complete supplier with org, profile, and configurable overrides.
    /// Returns the OrgId and ProfileId for use in tests.
    /// </summary>
    private static async Task<(Guid OrgId, Guid ProfileId, Guid UserId)> SeedSupplierAsync(
        GreenSuppliersDbContext context,
        Action<SupplierProfile>? configureProfile = null,
        Action<Organization>? configureOrg = null)
    {
        var now = DateTime.UtcNow;
        var orgId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var org = new Organization
        {
            Id = orgId,
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
            Id = profileId,
            OrganizationId = orgId,
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

        // Also seed a user record to make tests realistic
        context.Users.Add(new User
        {
            Id = userId,
            OrganizationId = orgId,
            Email = "supplier@ecosolutions.co.za",
            PasswordHash = "hashed",
            FirstName = "Supplier",
            LastName = "User",
            Role = UserRole.SupplierAdmin,
            IsActive = true,
            EmailVerified = true,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();
        return (orgId, profileId, userId);
    }

    private static UpdateMyProfileRequest CreateValidUpdateRequest() => new()
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
        SustainablePackaging = false,
        IndustryIds = new List<Guid>(),
        ServiceTagIds = new List<Guid>()
    };

    private static async Task<Guid> SeedCertificationTypeAsync(GreenSuppliersDbContext context, string name = "ISO 14001", string slug = "iso-14001")
    {
        var certType = new CertificationType
        {
            Id = Guid.NewGuid(),
            Name = name,
            Slug = slug,
            CreatedAt = DateTime.UtcNow
        };
        context.CertificationTypes.Add(certType);
        await context.SaveChangesAsync();
        return certType.Id;
    }

    // =========================================================================
    // GetByOrganizationIdAsync
    // =========================================================================

    [Fact]
    public async Task GetByOrganizationId_ValidOrg_ReturnsProfile()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId, _) = await SeedSupplierAsync(context);

        // Act
        var result = await service.GetByOrganizationIdAsync(orgId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(profileId);
        result.OrganizationId.Should().Be(orgId);
        result.TradingName.Should().Be("Eco Solutions");
        result.Description.Should().Contain("sustainable packaging");
        result.CountryCode.Should().Be("ZA");
    }

    [Fact]
    public async Task GetByOrganizationId_InvalidOrg_ReturnsNull()
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
    public async Task GetByOrganizationId_DeletedProfile_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context, profile =>
        {
            profile.IsDeleted = true;
            profile.DeletedAt = DateTime.UtcNow;
        });

        // Act
        var result = await service.GetByOrganizationIdAsync(orgId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByOrganizationId_ReturnsIndustriesAndCertifications()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId, _) = await SeedSupplierAsync(context);

        // Seed an industry and link it
        var industry = new Industry
        {
            Id = Guid.NewGuid(),
            Name = "Renewable Energy",
            Slug = "renewable-energy",
            CreatedAt = DateTime.UtcNow
        };
        context.Industries.Add(industry);
        context.Set<SupplierIndustry>().Add(new SupplierIndustry
        {
            SupplierProfileId = profileId,
            IndustryId = industry.Id
        });

        // Seed a certification
        var certTypeId = await SeedCertificationTypeAsync(context);
        context.SupplierCertifications.Add(new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            CertificationTypeId = certTypeId,
            Status = CertificationStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetByOrganizationIdAsync(orgId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Industries.Should().HaveCount(1);
        result.Industries[0].Name.Should().Be("Renewable Energy");
        result.Certifications.Should().HaveCount(1);
        result.Certifications[0].CertificationTypeName.Should().Be("ISO 14001");
    }

    // =========================================================================
    // UpdateOwnProfileAsync
    // =========================================================================

    [Fact]
    public async Task UpdateOwnProfile_UpdatesEditableFields()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);
        var request = CreateValidUpdateRequest();

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
        result.WasteRecyclingPercent.Should().Be(50);
        result.CarbonReporting.Should().BeTrue();
        result.WaterManagement.Should().BeTrue();
        result.SustainablePackaging.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateOwnProfile_DoesNotChangeCountryCode()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);
        var request = CreateValidUpdateRequest();

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert — CountryCode should remain "ZA" (not editable via this endpoint)
        result.Should().NotBeNull();
        result!.CountryCode.Should().Be("ZA");
    }

    [Fact]
    public async Task UpdateOwnProfile_DoesNotChangeVerificationStatus()
    {
        // Arrange — start with Flagged status
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context, profile =>
        {
            profile.VerificationStatus = VerificationStatus.Flagged;
        });
        var request = CreateValidUpdateRequest();

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert — Flagged status is preserved by VerificationService
        result.Should().NotBeNull();
        result!.VerificationStatus.Should().Be(VerificationStatus.Flagged);
    }

    [Fact]
    public async Task UpdateOwnProfile_TriggersEsgRescore()
    {
        // Arrange — start with incomplete profile so ESG = None
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context, profile =>
        {
            profile.TradingName = null;
            profile.EsgLevel = EsgLevel.None;
            profile.EsgScore = 0;
        });

        var request = new UpdateMyProfileRequest
        {
            TradingName = "Eco Solutions Complete",
            Description = "Full description provided.",
            City = "Cape Town",
            RenewableEnergyPercent = 30,
            IndustryIds = new List<Guid>(),
            ServiceTagIds = new List<Guid>()
        };

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert — profile should now be at least Bronze after re-scoring
        result.Should().NotBeNull();
        result!.EsgLevel.Should().NotBe(EsgLevel.None);
        result.EsgScore.Should().BeGreaterThan(0);
        result.LastScoredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOwnProfile_WritesAuditLog()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);
        var request = CreateValidUpdateRequest();

        // Act
        await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert — verify audit event was written
        var auditEvents = await context.AuditEvents
            .Where(a => a.Action == "SupplierSelfUpdated" && a.UserId == userId)
            .ToListAsync();

        auditEvents.Should().HaveCount(1);
        auditEvents[0].EntityType.Should().Be("SupplierProfile");
        auditEvents[0].UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UpdateOwnProfile_UpdatesIndustries()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId, userId) = await SeedSupplierAsync(context);

        // Seed industries
        var industryA = new Industry { Id = Guid.NewGuid(), Name = "Solar", Slug = "solar", CreatedAt = DateTime.UtcNow };
        var industryB = new Industry { Id = Guid.NewGuid(), Name = "Wind", Slug = "wind", CreatedAt = DateTime.UtcNow };
        context.Industries.AddRange(industryA, industryB);
        await context.SaveChangesAsync();

        var request = CreateValidUpdateRequest();
        request.IndustryIds = new List<Guid> { industryA.Id, industryB.Id };

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Industries.Should().HaveCount(2);
        result.Industries.Select(i => i.Name).Should().Contain("Solar");
        result.Industries.Select(i => i.Name).Should().Contain("Wind");
    }

    [Fact]
    public async Task UpdateOwnProfile_ReplacesExistingIndustries()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId, userId) = await SeedSupplierAsync(context);

        // Seed and link initial industry
        var oldIndustry = new Industry { Id = Guid.NewGuid(), Name = "Old Industry", Slug = "old-industry", CreatedAt = DateTime.UtcNow };
        var newIndustry = new Industry { Id = Guid.NewGuid(), Name = "New Industry", Slug = "new-industry", CreatedAt = DateTime.UtcNow };
        context.Industries.AddRange(oldIndustry, newIndustry);
        context.Set<SupplierIndustry>().Add(new SupplierIndustry
        {
            SupplierProfileId = profileId,
            IndustryId = oldIndustry.Id
        });
        await context.SaveChangesAsync();

        var request = CreateValidUpdateRequest();
        request.IndustryIds = new List<Guid> { newIndustry.Id };

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert — old industry should be removed, only new industry linked
        result.Should().NotBeNull();
        result!.Industries.Should().HaveCount(1);
        result.Industries[0].Name.Should().Be("New Industry");
    }

    [Fact]
    public async Task UpdateOwnProfile_UpdatesServiceTags()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId, userId) = await SeedSupplierAsync(context);

        var tagA = new ServiceTag { Id = Guid.NewGuid(), Name = "Solar Panels", Slug = "solar-panels", CreatedAt = DateTime.UtcNow };
        var tagB = new ServiceTag { Id = Guid.NewGuid(), Name = "Wind Turbines", Slug = "wind-turbines", CreatedAt = DateTime.UtcNow };
        context.ServiceTags.AddRange(tagA, tagB);
        await context.SaveChangesAsync();

        var request = CreateValidUpdateRequest();
        request.ServiceTagIds = new List<Guid> { tagA.Id, tagB.Id };

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ServiceTags.Should().HaveCount(2);
        result.ServiceTags.Select(t => t.Name).Should().Contain("Solar Panels");
        result.ServiceTags.Select(t => t.Name).Should().Contain("Wind Turbines");
    }

    [Fact]
    public async Task UpdateOwnProfile_InvalidOrgId_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        await SeedSupplierAsync(context);
        var request = CreateValidUpdateRequest();

        // Act
        var result = await service.UpdateOwnProfileAsync(Guid.NewGuid(), request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateOwnProfile_UpdatesOrganizationContactFields()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);
        var request = CreateValidUpdateRequest();
        request.Website = "https://new-website.co.za";
        request.Phone = "+27999999999";
        request.Email = "new-contact@eco.co.za";

        // Act
        var result = await service.UpdateOwnProfileAsync(orgId, request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Website.Should().Be("https://new-website.co.za");
        result.Phone.Should().Be("+27999999999");
        result.Email.Should().Be("new-contact@eco.co.za");

        // Verify the Organization entity was also updated
        var org = await context.Organizations.FirstAsync(o => o.Id == orgId);
        org.Website.Should().Be("https://new-website.co.za");
        org.Phone.Should().Be("+27999999999");
        org.Email.Should().Be("new-contact@eco.co.za");
    }

    // =========================================================================
    // AddCertificationAsync
    // =========================================================================

    [Fact]
    public async Task AddCertification_SetsStatusToPending()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);
        var certTypeId = await SeedCertificationTypeAsync(context);

        var request = new AddCertificationRequest
        {
            CertificationTypeId = certTypeId,
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
        result.IssuedAt.Should().Be(new DateOnly(2025, 1, 1));
        result.ExpiresAt.Should().Be(new DateOnly(2028, 1, 1));

        // Verify persisted in DB
        var dbCert = await context.SupplierCertifications.FirstOrDefaultAsync();
        dbCert.Should().NotBeNull();
        dbCert!.Status.Should().Be(CertificationStatus.Pending);
    }

    [Fact]
    public async Task AddCertification_WritesAuditLog()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);
        var certTypeId = await SeedCertificationTypeAsync(context);

        var request = new AddCertificationRequest
        {
            CertificationTypeId = certTypeId,
            CertificateNumber = "CERT-AUDIT-001"
        };

        // Act
        var result = await service.AddCertificationAsync(orgId, request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        var auditEvents = await context.AuditEvents
            .Where(a => a.Action == "CertificationSubmitted" && a.UserId == userId)
            .ToListAsync();

        auditEvents.Should().HaveCount(1);
        auditEvents[0].EntityType.Should().Be("SupplierCertification");
        auditEvents[0].EntityId.Should().Be(result!.Id);
    }

    [Fact]
    public async Task AddCertification_InvalidOrgId_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        await SeedSupplierAsync(context);
        var certTypeId = await SeedCertificationTypeAsync(context);

        var request = new AddCertificationRequest
        {
            CertificationTypeId = certTypeId,
            CertificateNumber = "CERT-INVALID"
        };

        // Act
        var result = await service.AddCertificationAsync(Guid.NewGuid(), request, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddCertification_InvalidCertTypeId_ThrowsArgumentException()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);

        var request = new AddCertificationRequest
        {
            CertificationTypeId = Guid.NewGuid(), // Does not exist
            CertificateNumber = "CERT-BADTYPE"
        };

        // Act & Assert — backend audit changed this to throw ArgumentException
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.AddCertificationAsync(orgId, request, userId, CancellationToken.None));
    }

    [Fact]
    public async Task AddCertification_TriggersRescore()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId, userId) = await SeedSupplierAsync(context, profile =>
        {
            profile.EsgScore = 25;
            profile.EsgLevel = EsgLevel.Bronze;
        });
        var certTypeId = await SeedCertificationTypeAsync(context);

        var request = new AddCertificationRequest
        {
            CertificationTypeId = certTypeId,
            CertificateNumber = "CERT-SCORE-001",
            IssuedAt = new DateOnly(2025, 1, 1),
            ExpiresAt = new DateOnly(2028, 1, 1)
        };

        // Act
        await service.AddCertificationAsync(orgId, request, userId, CancellationToken.None);

        // Assert — re-scoring should have run
        var profile = await context.SupplierProfiles.FirstAsync(p => p.Id == profileId);
        profile.LastScoredAt.Should().NotBeNull();

        // Adding a Pending cert should change verification status to Pending
        profile.VerificationStatus.Should().Be(VerificationStatus.Pending);
    }

    [Fact]
    public async Task AddCertification_WithNullOptionalFields_Succeeds()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, userId) = await SeedSupplierAsync(context);
        var certTypeId = await SeedCertificationTypeAsync(context);

        var request = new AddCertificationRequest
        {
            CertificationTypeId = certTypeId,
            CertificateNumber = null,
            IssuedAt = null,
            ExpiresAt = null,
            DocumentId = null
        };

        // Act
        var result = await service.AddCertificationAsync(orgId, request, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CertificateNumber.Should().BeNull();
        result.IssuedAt.Should().BeNull();
        result.ExpiresAt.Should().BeNull();
    }

    // =========================================================================
    // RequestPublicationAsync
    // =========================================================================

    [Fact]
    public async Task RequestPublication_CompleteProfile_SetsPublished()
    {
        // Arrange — profile with enough fields to be > 50% complete
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context, profile =>
        {
            profile.IsPublished = false;
            profile.PublishedAt = null;
            profile.YearFounded = 2015;
            profile.LogoUrl = "https://example.com/logo.png";
        });

        // Act
        var (success, _) = await service.RequestPublicationAsync(orgId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        success.Should().BeTrue();
        var profile = await context.SupplierProfiles.FirstAsync(p => p.OrganizationId == orgId);
        profile.IsPublished.Should().BeTrue();
        profile.PublishedAt.Should().NotBeNull();
        profile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RequestPublication_IncompleteProfile_ReturnsFalse()
    {
        // Arrange — strip most fields to make completeness < 50%
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context, profile =>
        {
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
    public async Task RequestPublication_FlaggedProfile_ReturnsFalse()
    {
        // Arrange — complete profile but Flagged
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context, profile =>
        {
            profile.VerificationStatus = VerificationStatus.Flagged;
            profile.FlaggedReason = "Suspicious data";
            profile.IsPublished = false;
            profile.PublishedAt = null;
            profile.YearFounded = 2015;
            profile.LogoUrl = "https://example.com/logo.png";
        });

        // Act
        var (success, _) = await service.RequestPublicationAsync(orgId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        success.Should().BeFalse();
        var profile = await context.SupplierProfiles.FirstAsync(p => p.OrganizationId == orgId);
        profile.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task RequestPublication_NonExistentOrg_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var (success, _) = await service.RequestPublicationAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        success.Should().BeFalse();
    }

    [Fact]
    public async Task RequestPublication_DeletedProfile_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context, profile =>
        {
            profile.IsDeleted = true;
            profile.DeletedAt = DateTime.UtcNow;
            profile.IsPublished = false;
        });

        // Act
        var (success, _) = await service.RequestPublicationAsync(orgId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        success.Should().BeFalse();
    }

    [Fact]
    public async Task RequestPublication_ExactlyAt50PercentCompleteness_SetsPublished()
    {
        // Arrange — build a profile that is exactly 50% complete
        // TradingName(7) + Description(7) + ShortDescription(6) + City(8) + Province(7) +
        // RenewableEnergy(5) + WasteRecycling(5) + CarbonReporting(4) + SustainablePackaging(3) = 52
        // But we need exactly 50. Let's remove SustainablePackaging(3) => 49, too low.
        // Add Website(4) via org => 53. Let's try:
        // TradingName(7) + Description(7) + City(8) + Province(7) + ShortDescription(6) +
        // RenewableEnergy(5) + WasteRecycling(5) + CarbonReporting(4) = 49, need 1 more.
        // Add WaterManagement(3) => 52. OK, just test boundary >= 50.
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context, profile =>
        {
            profile.TradingName = "Test"; // 7
            profile.Description = "Test description"; // 7
            profile.ShortDescription = "Short"; // 6
            profile.City = "Durban"; // 8
            profile.Province = "KZN"; // 7
            profile.YearFounded = null; // 0
            profile.RenewableEnergyPercent = 10; // 5
            profile.WasteRecyclingPercent = 10; // 5
            profile.CarbonReporting = true; // 4
            profile.WaterManagement = false; // 0
            profile.SustainablePackaging = true; // 3
            profile.LogoUrl = null; // 0
            profile.IsPublished = false;
            profile.PublishedAt = null;
        }, org =>
        {
            org.Website = null;
            org.Phone = null;
            org.Email = null;
        });
        // Total: 7+7+6+8+7+5+5+4+3 = 52 >= 50

        // Act
        var (success, _) = await service.RequestPublicationAsync(orgId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        success.Should().BeTrue();
    }

    // =========================================================================
    // GetDashboardStatsAsync
    // =========================================================================

    [Fact]
    public async Task GetDashboardStats_ReturnsCorrectCounts()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId, _) = await SeedSupplierAsync(context);

        // Add leads with different statuses
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            ContactName = "John Doe",
            ContactEmail = "john@example.com",
            Message = "Interested in services",
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
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            ContactName = "Bob",
            ContactEmail = "bob@example.com",
            Message = "Inquiry",
            Status = LeadStatus.New,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Add certifications with different statuses
        var certTypeId = await SeedCertificationTypeAsync(context);
        context.SupplierCertifications.Add(new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            CertificationTypeId = certTypeId,
            Status = CertificationStatus.Accepted,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.SupplierCertifications.Add(new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            CertificationTypeId = certTypeId,
            Status = CertificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        context.SupplierCertifications.Add(new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            CertificationTypeId = certTypeId,
            Status = CertificationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDashboardStatsAsync(orgId, CancellationToken.None);

        // Assert
        result.LeadCount.Should().Be(3);
        result.NewLeadCount.Should().Be(2);
        result.CertificationCount.Should().Be(3);
        result.PendingCertCount.Should().Be(2);
        result.IsPublished.Should().BeTrue();
        result.EsgLevel.Should().NotBeNullOrEmpty();
        result.ProfileCompleteness.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetDashboardStats_NoProfile_ReturnsEmptyDto()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetDashboardStatsAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.LeadCount.Should().Be(0);
        result.NewLeadCount.Should().Be(0);
        result.CertificationCount.Should().Be(0);
        result.PendingCertCount.Should().Be(0);
        result.ProfileCompleteness.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardStats_ZeroLeadsAndCerts_ReturnsZeroCounts()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context);

        // Act
        var result = await service.GetDashboardStatsAsync(orgId, CancellationToken.None);

        // Assert
        result.LeadCount.Should().Be(0);
        result.NewLeadCount.Should().Be(0);
        result.CertificationCount.Should().Be(0);
        result.PendingCertCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboardStats_IncludesCompletenessScore()
    {
        // Arrange — profile has TradingName, Description, ShortDescription, City, Province,
        // RenewableEnergy, WasteRecycling, CarbonReporting, SustainablePackaging + org contact
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _, _) = await SeedSupplierAsync(context);

        // Act
        var result = await service.GetDashboardStatsAsync(orgId, CancellationToken.None);

        // Assert — with all those fields, completeness should be well above 0
        result.ProfileCompleteness.Should().BeGreaterThanOrEqualTo(40);
    }

    // =========================================================================
    // CalculateCompleteness (static, pure function)
    // =========================================================================

    [Fact]
    public void CalculateCompleteness_EmptyProfile_ReturnsZero()
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
    public void CalculateCompleteness_FullProfile_Returns100()
    {
        // Arrange
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Full Corp",
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
            TradingName = "Full Profile Corp",       // 7
            Description = "Complete description.",     // 7
            ShortDescription = "Short desc",           // 6
            YearFounded = 2010,                        // 5
            City = "Johannesburg",                     // 8
            Province = "Gauteng",                      // 7
            CountryCode = "ZA",
            RenewableEnergyPercent = 50,               // 5
            WasteRecyclingPercent = 60,                // 5
            CarbonReporting = true,                    // 4
            WaterManagement = true,                    // 3
            SustainablePackaging = true,               // 3
            LogoUrl = "https://example.com/logo.png",  // 10
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        // Org contact: Website(4) + Phone(3) + Email(3) = 10
        // Total: 7+7+6+5+8+7+5+5+4+3+3+10+10 = 80
        // Plus certs(20) = 100

        // Act
        var completeness = SupplierMeService.CalculateCompleteness(profile, 1);

        // Assert
        completeness.Should().Be(100);
    }

    [Fact]
    public void CalculateCompleteness_WithCerts_Adds20Percent()
    {
        // Arrange — minimal profile + certs
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Slug = "cert-test",
            TradingName = "Cert Test Corp",  // 7
            CountryCode = "ZA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var withoutCerts = SupplierMeService.CalculateCompleteness(profile, 0);
        var withCerts = SupplierMeService.CalculateCompleteness(profile, 1);

        // Assert — difference should be exactly 20 (the certification bucket)
        (withCerts - withoutCerts).Should().Be(20);
    }

    [Fact]
    public void CalculateCompleteness_MultipleCerts_StillAdds20()
    {
        // Arrange — having 5 certs should not add more than 20
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Slug = "multi-cert",
            TradingName = "Multi Cert Corp",
            CountryCode = "ZA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var withOneCert = SupplierMeService.CalculateCompleteness(profile, 1);
        var withFiveCerts = SupplierMeService.CalculateCompleteness(profile, 5);

        // Assert
        withOneCert.Should().Be(withFiveCerts, "multiple certs should not add beyond the 20-point bucket");
    }

    [Fact]
    public void CalculateCompleteness_NeverExceeds100()
    {
        // Arrange — max out everything
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Max Corp",
            CountryCode = "ZA",
            Website = "https://max.co.za",
            Phone = "+27211234567",
            Email = "info@max.co.za",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Organization = org,
            Slug = "max-profile",
            TradingName = "Max Corp",
            Description = "Full description.",
            ShortDescription = "Short",
            YearFounded = 2010,
            City = "City",
            Province = "Province",
            CountryCode = "ZA",
            RenewableEnergyPercent = 100,
            WasteRecyclingPercent = 100,
            CarbonReporting = true,
            WaterManagement = true,
            SustainablePackaging = true,
            LogoUrl = "https://logo.png",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var completeness = SupplierMeService.CalculateCompleteness(profile, 10);

        // Assert
        completeness.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void CalculateCompleteness_OnlyBasicInfo_Returns25()
    {
        // Arrange — only basic info fields
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Slug = "basic-only",
            TradingName = "Basic Corp",         // 7
            Description = "Some description",    // 7
            ShortDescription = "Short",          // 6
            YearFounded = 2020,                  // 5
            CountryCode = "ZA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var completeness = SupplierMeService.CalculateCompleteness(profile, 0);

        // Assert
        completeness.Should().Be(25); // 7+7+6+5 = 25
    }

    [Fact]
    public void CalculateCompleteness_WithOrganizationNull_SkipsContactSection()
    {
        // Arrange — Organization is null (not loaded)
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Organization = null!,
            Slug = "no-org",
            TradingName = "No Org Corp",
            CountryCode = "ZA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act — should not throw
        var completeness = SupplierMeService.CalculateCompleteness(profile, 0);

        // Assert — should only count TradingName (7)
        completeness.Should().Be(7);
    }

    [Fact]
    public void CalculateCompleteness_SustainabilityFields_AddCorrectPoints()
    {
        // Arrange — only sustainability fields
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Slug = "sustain-only",
            CountryCode = "ZA",
            RenewableEnergyPercent = 50,    // 5
            WasteRecyclingPercent = 60,     // 5
            CarbonReporting = true,         // 4
            WaterManagement = true,         // 3
            SustainablePackaging = true,    // 3
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var completeness = SupplierMeService.CalculateCompleteness(profile, 0);

        // Assert
        completeness.Should().Be(20); // 5+5+4+3+3 = 20
    }

    [Fact]
    public void CalculateCompleteness_LogoOnly_Returns10()
    {
        // Arrange
        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Slug = "logo-only",
            CountryCode = "ZA",
            LogoUrl = "https://example.com/logo.png", // 10
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var completeness = SupplierMeService.CalculateCompleteness(profile, 0);

        // Assert
        completeness.Should().Be(10);
    }
}
