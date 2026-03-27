using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class ScoringRunnerTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static ScoringRunner CreateRunner(GreenSuppliersDbContext context)
    {
        var esgScoring = new EsgScoringService();
        var verification = new VerificationService();
        return new ScoringRunner(context, esgScoring, verification);
    }

    private static async Task<SupplierProfile> SeedTrackedProfileAsync(
        GreenSuppliersDbContext context,
        Action<SupplierProfile>? configure = null)
    {
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Org",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Organizations.Add(org);

        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Slug = $"test-{Guid.NewGuid():N}",
            TradingName = "Test Corp",
            Description = "A test supplier",
            CountryCode = "ZA",
            City = "Cape Town",
            VerificationStatus = VerificationStatus.Unverified,
            EsgLevel = EsgLevel.None,
            EsgScore = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        configure?.Invoke(profile);
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();

        // Return the tracked entity (not detached)
        return profile;
    }

    [Fact]
    public async Task RunScoringAsync_CompleteProfileNoCerts_SetsBronze()
    {
        // Arrange
        var context = CreateDbContext();
        var runner = CreateRunner(context);
        var profile = await SeedTrackedProfileAsync(context);

        // Act
        await runner.RunScoringAsync(profile, CancellationToken.None);

        // Assert
        profile.EsgLevel.Should().Be(EsgLevel.Bronze);
        profile.EsgScore.Should().Be(25);
        profile.VerificationStatus.Should().Be(VerificationStatus.Unverified);
        profile.LastScoredAt.Should().NotBeNull();
        profile.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task RunScoringAsync_WithAcceptedCerts_UpdatesVerificationAndEsg()
    {
        // Arrange
        var context = CreateDbContext();
        var runner = CreateRunner(context);
        var profile = await SeedTrackedProfileAsync(context, p =>
        {
            p.RenewableEnergyPercent = 30;
        });

        // Add an accepted certification
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
            SupplierProfileId = profile.Id,
            CertificationTypeId = certType.Id,
            Status = CertificationStatus.Accepted,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        await runner.RunScoringAsync(profile, CancellationToken.None);

        // Assert — 1 valid cert + renewable 30% >= 20% => Silver; profile complete + cert => Verified
        profile.EsgLevel.Should().Be(EsgLevel.Silver);
        profile.EsgScore.Should().Be(50);
        profile.VerificationStatus.Should().Be(VerificationStatus.Verified);
    }

    [Fact]
    public async Task RunScoringAsync_FlaggedProfile_StaysFlagged()
    {
        // Arrange
        var context = CreateDbContext();
        var runner = CreateRunner(context);
        var profile = await SeedTrackedProfileAsync(context, p =>
        {
            p.VerificationStatus = VerificationStatus.Flagged;
        });

        // Act
        await runner.RunScoringAsync(profile, CancellationToken.None);

        // Assert — flagged stays flagged
        profile.VerificationStatus.Should().Be(VerificationStatus.Flagged);
    }

    [Fact]
    public async Task RunScoringAsync_PersistsChangesToDatabase()
    {
        // Arrange
        var context = CreateDbContext();
        var runner = CreateRunner(context);
        var profile = await SeedTrackedProfileAsync(context);

        // Act
        await runner.RunScoringAsync(profile, CancellationToken.None);

        // Assert — reload from DB to verify persistence
        var dbProfile = await context.SupplierProfiles
            .AsNoTracking()
            .FirstAsync(p => p.Id == profile.Id);
        dbProfile.EsgLevel.Should().Be(EsgLevel.Bronze);
        dbProfile.EsgScore.Should().Be(25);
        dbProfile.LastScoredAt.Should().NotBeNull();
    }
}
