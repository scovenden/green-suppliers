using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class SdgServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static SdgService CreateService(GreenSuppliersDbContext context)
    {
        var audit = new AuditService(context);
        return new SdgService(context, audit);
    }

    private static async Task SeedSdgsAsync(GreenSuppliersDbContext context)
    {
        // Seed all 17 SDGs (simulating what HasData does in a real migration)
        var sdgs = new List<Sdg>
        {
            new() { Id = 1, Name = "No Poverty", Description = "End poverty in all its forms everywhere", Color = "#E5243B" },
            new() { Id = 2, Name = "Zero Hunger", Description = "End hunger", Color = "#DDA63A" },
            new() { Id = 3, Name = "Good Health and Well-Being", Description = "Ensure healthy lives", Color = "#4C9F38" },
            new() { Id = 4, Name = "Quality Education", Description = "Ensure inclusive education", Color = "#C5192D" },
            new() { Id = 5, Name = "Gender Equality", Description = "Achieve gender equality", Color = "#FF3A21" },
            new() { Id = 6, Name = "Clean Water and Sanitation", Description = "Ensure water availability", Color = "#26BDE2" },
            new() { Id = 7, Name = "Affordable and Clean Energy", Description = "Ensure energy access", Color = "#FCC30B" },
            new() { Id = 8, Name = "Decent Work and Economic Growth", Description = "Promote economic growth", Color = "#A21942" },
            new() { Id = 9, Name = "Industry, Innovation and Infrastructure", Description = "Build infrastructure", Color = "#FD6925" },
            new() { Id = 10, Name = "Reduced Inequalities", Description = "Reduce inequality", Color = "#DD1367" },
            new() { Id = 11, Name = "Sustainable Cities and Communities", Description = "Make cities sustainable", Color = "#FD9D24" },
            new() { Id = 12, Name = "Responsible Consumption and Production", Description = "Sustainable consumption", Color = "#BF8B2E" },
            new() { Id = 13, Name = "Climate Action", Description = "Combat climate change", Color = "#3F7E44" },
            new() { Id = 14, Name = "Life Below Water", Description = "Conserve oceans", Color = "#0A97D9" },
            new() { Id = 15, Name = "Life on Land", Description = "Protect ecosystems", Color = "#56C02B" },
            new() { Id = 16, Name = "Peace, Justice and Strong Institutions", Description = "Promote peace", Color = "#00689D" },
            new() { Id = 17, Name = "Partnerships for the Goals", Description = "Strengthen partnerships", Color = "#19486A" },
        };
        context.Sdgs.AddRange(sdgs);
        await context.SaveChangesAsync();
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
    public async Task GetAllAsync_ReturnsAll17Sdgs()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var service = CreateService(context);

        var sdgs = await service.GetAllAsync(CancellationToken.None);

        sdgs.Should().HaveCount(17);
        sdgs.First().Id.Should().Be(1);
        sdgs.First().Name.Should().Be("No Poverty");
        sdgs.Last().Id.Should().Be(17);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOrderedById()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var service = CreateService(context);

        var sdgs = await service.GetAllAsync(CancellationToken.None);

        sdgs.Select(s => s.Id).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task UpdateSupplierSdgsAsync_AssignsNewSdgs()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var result = await service.UpdateSupplierSdgsAsync(
            profile.Id, new List<int> { 7, 13 }, Guid.NewGuid(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Select(s => s.Id).Should().BeEquivalentTo(new[] { 7, 13 });
    }

    [Fact]
    public async Task UpdateSupplierSdgsAsync_ReplacesExistingSdgs()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        // First assignment
        await service.UpdateSupplierSdgsAsync(profile.Id, new List<int> { 7, 13 }, Guid.NewGuid(), CancellationToken.None);

        // Second assignment should replace
        var result = await service.UpdateSupplierSdgsAsync(
            profile.Id, new List<int> { 1, 2, 3 }, Guid.NewGuid(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(s => s.Id).Should().BeEquivalentTo(new[] { 1, 2, 3 });
    }

    [Fact]
    public async Task UpdateSupplierSdgsAsync_EmptyList_ClearsAllSdgs()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        await service.UpdateSupplierSdgsAsync(profile.Id, new List<int> { 7, 13 }, Guid.NewGuid(), CancellationToken.None);

        var result = await service.UpdateSupplierSdgsAsync(
            profile.Id, new List<int>(), Guid.NewGuid(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateSupplierSdgsAsync_InvalidSdgId_ThrowsArgumentException()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var act = () => service.UpdateSupplierSdgsAsync(
            profile.Id, new List<int> { 0, 18 }, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid SDG IDs*");
    }

    [Fact]
    public async Task UpdateSupplierSdgsAsync_NonexistentProfile_ThrowsKeyNotFound()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var service = CreateService(context);

        var act = () => service.UpdateSupplierSdgsAsync(
            Guid.NewGuid(), new List<int> { 7 }, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateSupplierSdgsAsync_DuplicateIds_DeduplicatesAutomatically()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);

        var result = await service.UpdateSupplierSdgsAsync(
            profile.Id, new List<int> { 7, 7, 13, 13 }, Guid.NewGuid(), CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateSupplierSdgsAsync_WritesAuditLog()
    {
        var context = CreateDbContext();
        await SeedSdgsAsync(context);
        var profile = await SeedSupplierProfileAsync(context);
        var service = CreateService(context);
        var userId = Guid.NewGuid();

        await service.UpdateSupplierSdgsAsync(profile.Id, new List<int> { 7 }, userId, CancellationToken.None);

        var audit = await context.AuditEvents.FirstOrDefaultAsync(a => a.Action == "SdgsUpdated");
        audit.Should().NotBeNull();
        audit!.EntityId.Should().Be(profile.Id);
    }
}
