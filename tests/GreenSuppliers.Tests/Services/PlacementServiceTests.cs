using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class PlacementServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static PlacementService CreateService(GreenSuppliersDbContext context)
    {
        var audit = new AuditService(context);
        return new PlacementService(context, audit);
    }

    private static async Task<SupplierProfile> SeedSupplierProfileAsync(GreenSuppliersDbContext context)
    {
        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Solar Corp",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Organizations.Add(org);

        var profile = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Slug = "solar-corp",
            TradingName = "Solar Corp",
            CountryCode = "ZA",
            IsPublished = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();
        return profile;
    }

    [Fact]
    public async Task CreateAsync_CreatesPlacement()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var result = await service.CreateAsync(new CreatePlacementRequest
        {
            SupplierProfileId = profile.Id,
            PlacementType = "homepage-banner",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(30)
        }, Guid.NewGuid(), CancellationToken.None);

        result.Should().NotBeNull();
        result.SupplierProfileId.Should().Be(profile.Id);
        result.PlacementType.Should().Be("homepage-banner");
        result.IsActive.Should().BeTrue();
        result.ImpressionsCount.Should().Be(0);
        result.ClicksCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_NonexistentProfile_ThrowsKeyNotFound()
    {
        var context = CreateDbContext();
        var service = CreateService(context);

        var act = () => service.CreateAsync(new CreatePlacementRequest
        {
            SupplierProfileId = Guid.NewGuid(),
            PlacementType = "homepage-banner",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(30)
        }, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_EndBeforeStart_ThrowsArgumentException()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var act = () => service.CreateAsync(new CreatePlacementRequest
        {
            SupplierProfileId = profile.Id,
            PlacementType = "homepage-banner",
            StartsAt = DateTime.UtcNow.AddDays(30),
            EndsAt = DateTime.UtcNow
        }, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*End date must be after start date*");
    }

    [Fact]
    public async Task GetFeaturedAsync_ReturnsOnlyActivePlacements()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var now = DateTime.UtcNow;

        // Active placement (within date range, IsActive=true, supplier published)
        context.SponsoredPlacements.Add(new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            PlacementType = "homepage-banner",
            StartsAt = now.AddDays(-5),
            EndsAt = now.AddDays(25),
            IsActive = true,
            CreatedAt = now
        });

        // Inactive placement
        context.SponsoredPlacements.Add(new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            PlacementType = "sidebar",
            StartsAt = now.AddDays(-5),
            EndsAt = now.AddDays(25),
            IsActive = false,
            CreatedAt = now
        });

        // Expired placement
        context.SponsoredPlacements.Add(new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            PlacementType = "expired",
            StartsAt = now.AddDays(-30),
            EndsAt = now.AddDays(-1),
            IsActive = true,
            CreatedAt = now
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);
        var featured = await service.GetFeaturedAsync(CancellationToken.None);

        featured.Should().HaveCount(1);
        featured[0].PlacementType.Should().Be("homepage-banner");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var now = DateTime.UtcNow;
        var placement = new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            PlacementType = "homepage-banner",
            StartsAt = now,
            EndsAt = now.AddDays(30),
            IsActive = true,
            CreatedAt = now
        };
        context.SponsoredPlacements.Add(placement);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.UpdateAsync(placement.Id, new UpdatePlacementRequest
        {
            IsActive = false,
            PlacementType = "sidebar"
        }, Guid.NewGuid(), CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeFalse();
        result.PlacementType.Should().Be("sidebar");
    }

    [Fact]
    public async Task UpdateAsync_NonexistentPlacement_ReturnsNull()
    {
        var context = CreateDbContext();
        var service = CreateService(context);

        var result = await service.UpdateAsync(Guid.NewGuid(), new UpdatePlacementRequest
        {
            IsActive = false
        }, Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RecordImpressionAsync_IncrementsCount()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var now = DateTime.UtcNow;
        var placement = new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            PlacementType = "homepage-banner",
            StartsAt = now,
            EndsAt = now.AddDays(30),
            IsActive = true,
            ImpressionsCount = 5,
            CreatedAt = now
        };
        context.SponsoredPlacements.Add(placement);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.RecordImpressionAsync(placement.Id, CancellationToken.None);

        var updated = await context.SponsoredPlacements.FindAsync(placement.Id);
        updated!.ImpressionsCount.Should().Be(6);
    }

    [Fact]
    public async Task RecordClickAsync_IncrementsCount()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var now = DateTime.UtcNow;
        var placement = new SponsoredPlacement
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            PlacementType = "homepage-banner",
            StartsAt = now,
            EndsAt = now.AddDays(30),
            IsActive = true,
            ClicksCount = 10,
            CreatedAt = now
        };
        context.SponsoredPlacements.Add(placement);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.RecordClickAsync(placement.Id, CancellationToken.None);

        var updated = await context.SponsoredPlacements.FindAsync(placement.Id);
        updated!.ClicksCount.Should().Be(11);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedResults()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var now = DateTime.UtcNow;

        for (int i = 0; i < 5; i++)
        {
            context.SponsoredPlacements.Add(new SponsoredPlacement
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = profile.Id,
                PlacementType = $"type-{i}",
                StartsAt = now,
                EndsAt = now.AddDays(30),
                IsActive = true,
                CreatedAt = now.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetAllAsync(1, 2, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
        result.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task CreateAsync_WritesAuditLog()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);
        var adminUserId = Guid.NewGuid();

        await service.CreateAsync(new CreatePlacementRequest
        {
            SupplierProfileId = profile.Id,
            PlacementType = "homepage-banner",
            StartsAt = DateTime.UtcNow,
            EndsAt = DateTime.UtcNow.AddDays(30)
        }, adminUserId, CancellationToken.None);

        var audit = await context.AuditEvents.FirstOrDefaultAsync(a => a.Action == "PlacementCreated");
        audit.Should().NotBeNull();
    }
}
