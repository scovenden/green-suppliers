using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class SupplierServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static SupplierService CreateService(GreenSuppliersDbContext context)
    {
        var esgScoring = new EsgScoringService();
        var verification = new VerificationService();
        var audit = new AuditService(context);
        return new SupplierService(context, esgScoring, verification, audit);
    }

    private static CreateSupplierRequest CreateValidRequest() => new()
    {
        CompanyName = "Eco Solutions (Pty) Ltd",
        TradingName = "Eco Solutions",
        Description = "A leading provider of sustainable packaging solutions.",
        ShortDescription = "Sustainable packaging",
        CountryCode = "ZA",
        City = "Cape Town",
        Province = "Western Cape",
        Website = "https://ecosolutions.co.za",
        Phone = "+27211234567",
        Email = "info@ecosolutions.co.za",
        RenewableEnergyPercent = 30,
        WasteRecyclingPercent = 40,
        CarbonReporting = true,
        WaterManagement = false,
        SustainablePackaging = true
    };

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesOrgAndProfile()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var request = CreateValidRequest();
        var adminUserId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(request, adminUserId);

        // Assert
        result.Should().NotBeNull();
        result.OrganizationName.Should().Be("Eco Solutions (Pty) Ltd");
        result.TradingName.Should().Be("Eco Solutions");
        result.Description.Should().Be("A leading provider of sustainable packaging solutions.");
        result.CountryCode.Should().Be("ZA");
        result.City.Should().Be("Cape Town");
        result.CarbonReporting.Should().BeTrue();
        result.SustainablePackaging.Should().BeTrue();

        // Verify org was created
        var org = await context.Organizations.FirstOrDefaultAsync();
        org.Should().NotBeNull();
        org!.Name.Should().Be("Eco Solutions (Pty) Ltd");
        org.OrganizationType.Should().Be(OrganizationType.Supplier);

        // Verify profile was created
        var profile = await context.SupplierProfiles.FirstOrDefaultAsync();
        profile.Should().NotBeNull();
        profile!.OrganizationId.Should().Be(org.Id);
        profile.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_GeneratesSlugFromTradingName()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var request = CreateValidRequest();
        request.TradingName = "Eco Solutions SA";
        var adminUserId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(request, adminUserId);

        // Assert
        result.Slug.Should().Be("eco-solutions-sa");
    }

    [Fact]
    public async Task CreateAsync_GeneratesSlugFromCompanyName_WhenNoTradingName()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var request = CreateValidRequest();
        request.TradingName = null;
        request.CompanyName = "Green Energy Corp";
        var adminUserId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(request, adminUserId);

        // Assert
        result.Slug.Should().Be("green-energy-corp");
    }

    [Fact]
    public async Task CreateAsync_DuplicateSlug_AppendsSuffix()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        // Create first supplier
        var request1 = CreateValidRequest();
        request1.TradingName = "Eco Solutions";
        await service.CreateAsync(request1, adminUserId);

        // Create second supplier with same name
        var request2 = CreateValidRequest();
        request2.TradingName = "Eco Solutions";

        // Act
        var result = await service.CreateAsync(request2, adminUserId);

        // Assert
        result.Slug.Should().Be("eco-solutions-2");
    }

    [Fact]
    public async Task CreateAsync_RunsEsgScoringOnCreate()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var request = CreateValidRequest();
        // Complete profile with renewable energy >= 20 but no certs => Bronze
        request.RenewableEnergyPercent = 30;
        var adminUserId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(request, adminUserId);

        // Assert — profile is complete so at minimum Bronze
        result.EsgLevel.Should().Be(EsgLevel.Bronze);
        result.EsgScore.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateAsync_WritesAuditLog()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var request = CreateValidRequest();
        var adminUserId = Guid.NewGuid();

        // Act
        await service.CreateAsync(request, adminUserId);

        // Assert
        var auditEvent = await context.AuditEvents.FirstOrDefaultAsync();
        auditEvent.Should().NotBeNull();
        auditEvent!.Action.Should().Be("SupplierCreated");
        auditEvent.EntityType.Should().Be("SupplierProfile");
        auditEvent.UserId.Should().Be(adminUserId);
    }

    [Fact]
    public async Task CreateAsync_LinksIndustries()
    {
        // Arrange
        var context = CreateDbContext();
        var industry = new Industry
        {
            Id = Guid.NewGuid(),
            Name = "Renewable Energy",
            Slug = "renewable-energy",
            CreatedAt = DateTime.UtcNow
        };
        context.Industries.Add(industry);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var request = CreateValidRequest();
        request.IndustryIds = new List<Guid> { industry.Id };
        var adminUserId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(request, adminUserId);

        // Assert
        result.Industries.Should().HaveCount(1);
        result.Industries[0].Name.Should().Be("Renewable Energy");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesProfileAndRescores()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        var created = await service.CreateAsync(CreateValidRequest(), adminUserId);

        var updateRequest = new UpdateSupplierRequest
        {
            CompanyName = "Eco Solutions Updated",
            TradingName = "Eco Updated",
            Description = "Updated description for the company.",
            CountryCode = "ZA",
            City = "Johannesburg",
            RenewableEnergyPercent = 60,
            CarbonReporting = true
        };

        // Act
        var result = await service.UpdateAsync(created.Id, updateRequest, adminUserId);

        // Assert
        result.Should().NotBeNull();
        result!.TradingName.Should().Be("Eco Updated");
        result.City.Should().Be("Johannesburg");
        result.OrganizationName.Should().Be("Eco Solutions Updated");
        // With complete profile, carbon reporting, 60% renewable but no certs => Bronze
        result.EsgLevel.Should().Be(EsgLevel.Bronze);
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdateSupplierRequest
        {
            CompanyName = "Test",
            CountryCode = "ZA"
        }, adminUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ReturnsPublishedProfile()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        var created = await service.CreateAsync(CreateValidRequest(), adminUserId);

        // Act
        var result = await service.GetBySlugAsync(created.Slug);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.TradingName.Should().Be("Eco Solutions");
        result.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task GetBySlugAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetBySlugAsync("non-existent-slug");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_UnpublishedProfile_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        var created = await service.CreateAsync(CreateValidRequest(), adminUserId);
        await service.SetPublishedAsync(created.Id, false, adminUserId);

        // Act
        var result = await service.GetBySlugAsync(created.Slug);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsProfileIncludingUnpublished()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        var created = await service.CreateAsync(CreateValidRequest(), adminUserId);
        await service.SetPublishedAsync(created.Id, false, adminUserId);

        // Act
        var result = await service.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task SetVerificationStatusAsync_Flagged_UpdatesStatus()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        var created = await service.CreateAsync(CreateValidRequest(), adminUserId);

        // Act
        var success = await service.SetVerificationStatusAsync(
            created.Id, VerificationStatus.Flagged, "Suspicious activity", adminUserId);

        // Assert
        success.Should().BeTrue();

        var profile = await context.SupplierProfiles.FindAsync(created.Id);
        profile!.VerificationStatus.Should().Be(VerificationStatus.Flagged);
        profile.FlaggedReason.Should().Be("Suspicious activity");

        // Verify audit log
        var auditEvent = await context.AuditEvents
            .Where(a => a.Action == "VerificationStatusChanged")
            .FirstOrDefaultAsync();
        auditEvent.Should().NotBeNull();
    }

    [Fact]
    public async Task SetPublishedAsync_PublishesProfile()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        var created = await service.CreateAsync(CreateValidRequest(), adminUserId);
        await service.SetPublishedAsync(created.Id, false, adminUserId);

        // Act
        var success = await service.SetPublishedAsync(created.Id, true, adminUserId);

        // Assert
        success.Should().BeTrue();
        var profile = await context.SupplierProfiles.FindAsync(created.Id);
        profile!.IsPublished.Should().BeTrue();
        profile.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RescoreAsync_UpdatesEsgLevelAndScore()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        var created = await service.CreateAsync(CreateValidRequest(), adminUserId);

        // Manually modify the profile to have higher stats, then rescore
        var profile = await context.SupplierProfiles.FindAsync(created.Id);
        profile!.RenewableEnergyPercent = 80;
        profile.WasteRecyclingPercent = 80;
        profile.CarbonReporting = true;
        await context.SaveChangesAsync();

        // Add 3 valid certifications
        var certType = new CertificationType
        {
            Id = Guid.NewGuid(),
            Name = "ISO 14001",
            Slug = "iso-14001",
            CreatedAt = DateTime.UtcNow
        };
        context.CertificationTypes.Add(certType);

        for (int i = 0; i < 3; i++)
        {
            context.SupplierCertifications.Add(new SupplierCertification
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = profile.Id,
                CertificationTypeId = certType.Id,
                Status = CertificationStatus.Accepted,
                ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        // Act
        var success = await service.RescoreAsync(created.Id);

        // Assert
        success.Should().BeTrue();
        var updated = await context.SupplierProfiles.FindAsync(created.Id);
        updated!.EsgLevel.Should().Be(EsgLevel.Platinum);
        updated.EsgScore.Should().Be(100);
        updated.LastScoredAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RescoreAsync_NonExistent_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.RescoreAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }
}
