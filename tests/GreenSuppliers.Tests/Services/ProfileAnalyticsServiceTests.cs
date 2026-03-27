using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace GreenSuppliers.Tests.Services;

public class ProfileAnalyticsServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static ProfileAnalyticsService CreateService(GreenSuppliersDbContext context)
    {
        return new ProfileAnalyticsService(context, NullLogger<ProfileAnalyticsService>.Instance);
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
    public async Task RecordViewAsync_CreatesProfileViewRecord()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        await service.RecordViewAsync(profile.Id, "127.0.0.1", null, "https://google.com", CancellationToken.None);

        var views = await context.ProfileViews.ToListAsync();
        views.Should().HaveCount(1);
        views[0].SupplierProfileId.Should().Be(profile.Id);
        views[0].ViewerIp.Should().Be("127.0.0.1");
        views[0].Referrer.Should().Be("https://google.com");
    }

    [Fact]
    public async Task RecordViewAsync_WithUserId_RecordsViewerUserId()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);
        var userId = Guid.NewGuid();

        await service.RecordViewAsync(profile.Id, "127.0.0.1", userId, null, CancellationToken.None);

        var view = await context.ProfileViews.FirstAsync();
        view.ViewerUserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetAnalyticsAsync_EmptyData_ReturnsZeroes()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var analytics = await service.GetAnalyticsAsync(profile.Id, CancellationToken.None);

        analytics.TotalViews.Should().Be(0);
        analytics.ViewsThisMonth.Should().Be(0);
        analytics.ViewsByDay.Should().BeEmpty();
        analytics.TotalLeads.Should().Be(0);
        analytics.LeadsByMonth.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithViews_ReturnsTotalViews()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        // Add 3 views
        for (int i = 0; i < 3; i++)
        {
            context.ProfileViews.Add(new ProfileView
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = profile.Id,
                ViewerIp = "127.0.0.1",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
        await context.SaveChangesAsync();

        var analytics = await service.GetAnalyticsAsync(profile.Id, CancellationToken.None);

        analytics.TotalViews.Should().Be(3);
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithViews_ReturnsCorrectViewsThisMonth()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var now = DateTime.UtcNow;

        // 2 views this month
        context.ProfileViews.Add(new ProfileView
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            ViewerIp = "127.0.0.1",
            CreatedAt = now
        });
        context.ProfileViews.Add(new ProfileView
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            ViewerIp = "127.0.0.2",
            CreatedAt = now.AddDays(-1)
        });
        // 1 view last month
        context.ProfileViews.Add(new ProfileView
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            ViewerIp = "127.0.0.3",
            CreatedAt = now.AddMonths(-1).AddDays(-1)
        });
        await context.SaveChangesAsync();

        var analytics = await service.GetAnalyticsAsync(profile.Id, CancellationToken.None);

        analytics.TotalViews.Should().Be(3);
        analytics.ViewsThisMonth.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAnalyticsAsync_ViewsByDay_GroupsByDate()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var today = DateTime.UtcNow.Date;

        // 2 views today, 1 yesterday
        context.ProfileViews.Add(new ProfileView { Id = Guid.NewGuid(), SupplierProfileId = profile.Id, CreatedAt = today.AddHours(10) });
        context.ProfileViews.Add(new ProfileView { Id = Guid.NewGuid(), SupplierProfileId = profile.Id, CreatedAt = today.AddHours(14) });
        context.ProfileViews.Add(new ProfileView { Id = Guid.NewGuid(), SupplierProfileId = profile.Id, CreatedAt = today.AddDays(-1).AddHours(10) });
        await context.SaveChangesAsync();

        var analytics = await service.GetAnalyticsAsync(profile.Id, CancellationToken.None);

        analytics.ViewsByDay.Should().HaveCount(2);
        analytics.ViewsByDay.First(v => v.Date == DateOnly.FromDateTime(today)).Count.Should().Be(2);
    }

    [Fact]
    public async Task GetAnalyticsAsync_WithLeads_ReturnsTotalLeads()
    {
        var context = CreateDbContext();
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            ContactName = "Test Buyer",
            ContactEmail = "buyer@test.com",
            Message = "Interested in your services",
            Status = LeadStatus.New,
            CreatedAt = DateTime.UtcNow
        });
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            ContactName = "Another Buyer",
            ContactEmail = "buyer2@test.com",
            Message = "Please contact me",
            Status = LeadStatus.Contacted,
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        });
        await context.SaveChangesAsync();

        var analytics = await service.GetAnalyticsAsync(profile.Id, CancellationToken.None);

        analytics.TotalLeads.Should().Be(2);
    }

    [Fact]
    public async Task GetAnalyticsAsync_DoesNotCountOtherSuppliersViews()
    {
        var context = CreateDbContext();
        var profile1 = await SeedSupplierProfileAsync(context);

        // Create a second profile
        var now = DateTime.UtcNow;
        var org2 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Wind Corp",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Organizations.Add(org2);
        var profile2 = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org2.Id,
            Slug = "wind-corp",
            TradingName = "Wind Corp",
            CountryCode = "ZA",
            IsPublished = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.SupplierProfiles.Add(profile2);

        // Views for profile1
        context.ProfileViews.Add(new ProfileView { Id = Guid.NewGuid(), SupplierProfileId = profile1.Id, CreatedAt = now });
        // Views for profile2
        context.ProfileViews.Add(new ProfileView { Id = Guid.NewGuid(), SupplierProfileId = profile2.Id, CreatedAt = now });
        context.ProfileViews.Add(new ProfileView { Id = Guid.NewGuid(), SupplierProfileId = profile2.Id, CreatedAt = now });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var analytics = await service.GetAnalyticsAsync(profile1.Id, CancellationToken.None);

        analytics.TotalViews.Should().Be(1);
    }
}
