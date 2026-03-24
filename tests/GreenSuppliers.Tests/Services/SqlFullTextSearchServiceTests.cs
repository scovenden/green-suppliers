using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class SqlFullTextSearchServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static SqlFullTextSearchService CreateService(GreenSuppliersDbContext context)
    {
        return new SqlFullTextSearchService(context);
    }

    private static async Task SeedTestData(GreenSuppliersDbContext context)
    {
        // Industries
        var renewableEnergy = new Industry
        {
            Id = Guid.NewGuid(),
            Name = "Renewable Energy",
            Slug = "renewable-energy",
            CreatedAt = DateTime.UtcNow
        };
        var construction = new Industry
        {
            Id = Guid.NewGuid(),
            Name = "Sustainable Construction",
            Slug = "sustainable-construction",
            CreatedAt = DateTime.UtcNow
        };
        context.Industries.AddRange(renewableEnergy, construction);

        // Service tags
        var solarTag = new ServiceTag
        {
            Id = Guid.NewGuid(),
            Name = "Solar Panels",
            Slug = "solar-panels",
            CreatedAt = DateTime.UtcNow
        };
        var windTag = new ServiceTag
        {
            Id = Guid.NewGuid(),
            Name = "Wind Turbines",
            Slug = "wind-turbines",
            CreatedAt = DateTime.UtcNow
        };
        context.ServiceTags.AddRange(solarTag, windTag);

        // Certification types
        var iso14001 = new CertificationType
        {
            Id = Guid.NewGuid(),
            Name = "ISO 14001",
            Slug = "iso-14001",
            CreatedAt = DateTime.UtcNow
        };
        context.CertificationTypes.Add(iso14001);

        // Organizations
        var org1 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Solar Corp",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var org2 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Wind Power Ltd",
            CountryCode = "KE",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var org3 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Green Build Co",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var org4 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Hidden Corp",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var org5 = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Corp",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.Organizations.AddRange(org1, org2, org3, org4, org5);

        // Supplier profiles
        var profile1 = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org1.Id,
            Slug = "solar-corp",
            TradingName = "Solar Corp",
            Description = "Leading solar energy provider in South Africa",
            ShortDescription = "Solar energy solutions",
            CountryCode = "ZA",
            City = "Cape Town",
            EsgLevel = EsgLevel.Gold,
            EsgScore = 75,
            VerificationStatus = VerificationStatus.Verified,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            UpdatedAt = DateTime.UtcNow
        };
        var profile2 = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org2.Id,
            Slug = "wind-power",
            TradingName = "Wind Power Ltd",
            Description = "Wind energy solutions across East Africa",
            ShortDescription = "Wind turbine installation",
            CountryCode = "KE",
            City = "Nairobi",
            EsgLevel = EsgLevel.Silver,
            EsgScore = 50,
            VerificationStatus = VerificationStatus.Verified,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow
        };
        var profile3 = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org3.Id,
            Slug = "green-build",
            TradingName = "Green Build Co",
            Description = "Sustainable construction materials and services",
            ShortDescription = "Green building materials",
            CountryCode = "ZA",
            City = "Johannesburg",
            EsgLevel = EsgLevel.Platinum,
            EsgScore = 95,
            VerificationStatus = VerificationStatus.Verified,
            IsPublished = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };
        // Unpublished profile
        var profile4 = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org4.Id,
            Slug = "hidden-corp",
            TradingName = "Hidden Corp",
            Description = "This should not appear in search",
            ShortDescription = "Hidden",
            CountryCode = "ZA",
            City = "Pretoria",
            EsgLevel = EsgLevel.Bronze,
            EsgScore = 25,
            VerificationStatus = VerificationStatus.Unverified,
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        // Soft-deleted profile
        var profile5 = new SupplierProfile
        {
            Id = Guid.NewGuid(),
            OrganizationId = org5.Id,
            Slug = "deleted-corp",
            TradingName = "Deleted Corp",
            Description = "This was soft deleted",
            ShortDescription = "Deleted",
            CountryCode = "ZA",
            City = "Durban",
            EsgLevel = EsgLevel.Bronze,
            EsgScore = 20,
            VerificationStatus = VerificationStatus.Verified,
            IsPublished = true,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.SupplierProfiles.AddRange(profile1, profile2, profile3, profile4, profile5);

        // Link industries
        context.Set<SupplierIndustry>().AddRange(
            new SupplierIndustry { SupplierProfileId = profile1.Id, IndustryId = renewableEnergy.Id },
            new SupplierIndustry { SupplierProfileId = profile2.Id, IndustryId = renewableEnergy.Id },
            new SupplierIndustry { SupplierProfileId = profile3.Id, IndustryId = construction.Id }
        );

        // Link service tags
        context.Set<SupplierServiceTag>().AddRange(
            new SupplierServiceTag { SupplierProfileId = profile1.Id, ServiceTagId = solarTag.Id },
            new SupplierServiceTag { SupplierProfileId = profile2.Id, ServiceTagId = windTag.Id }
        );

        // Add accepted certification to profile1
        context.SupplierCertifications.Add(new SupplierCertification
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile1.Id,
            CertificationTypeId = iso14001.Id,
            Status = CertificationStatus.Accepted,
            ExpiresAt = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Search_NoFilters_ReturnsAllPublished()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery();

        // Act
        var result = await service.SearchAsync(query);

        // Assert — 3 published, non-deleted profiles
        result.Total.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Search_ByCountry_FiltersCorrectly()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { CountryCode = "ZA" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — Solar Corp and Green Build are ZA, published, non-deleted
        result.Total.Should().Be(2);
        result.Items.Should().AllSatisfy(i => i.CountryCode.Should().Be("ZA"));
    }

    [Fact]
    public async Task Search_ByIndustry_FiltersCorrectly()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { IndustrySlug = "renewable-energy" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — Solar Corp and Wind Power are in renewable energy
        result.Total.Should().Be(2);
        result.Items.Should().Contain(i => i.TradingName == "Solar Corp");
        result.Items.Should().Contain(i => i.TradingName == "Wind Power Ltd");
    }

    [Fact]
    public async Task Search_ByEsgLevel_FiltersCorrectly()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { EsgLevel = "Gold" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — only Solar Corp is Gold
        result.Total.Should().Be(1);
        result.Items[0].TradingName.Should().Be("Solar Corp");
        result.Items[0].EsgLevel.Should().Be("Gold");
    }

    [Fact]
    public async Task Search_ByTextQuery_MatchesNameAndDescription()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { Q = "solar" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — Solar Corp matches on TradingName and Description/ShortDescription
        result.Total.Should().Be(1);
        result.Items[0].TradingName.Should().Be("Solar Corp");
    }

    [Fact]
    public async Task Search_Pagination_ReturnsCorrectPage()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { Page = 2, PageSize = 2 };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — 3 total, page 2 with size 2 = 1 item
        result.Total.Should().Be(3);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_ExcludesUnpublished()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { Q = "Hidden" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — Hidden Corp is unpublished, should not appear
        result.Total.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_ExcludesSoftDeleted()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { Q = "Deleted" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — Deleted Corp is soft deleted, should not appear
        result.Total.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Search_SortByEsgScore_DescendingDefault()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { SortBy = "esgScore" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — Platinum (95) > Gold (75) > Silver (50)
        result.Items.Should().HaveCount(3);
        result.Items[0].EsgScore.Should().Be(95);
        result.Items[1].EsgScore.Should().Be(75);
        result.Items[2].EsgScore.Should().Be(50);
    }

    [Fact]
    public async Task Search_SortByName_Ascending()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { SortBy = "name" };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — alphabetical: Green Build Co, Solar Corp, Wind Power Ltd
        result.Items.Should().HaveCount(3);
        result.Items[0].TradingName.Should().Be("Green Build Co");
        result.Items[1].TradingName.Should().Be("Solar Corp");
        result.Items[2].TradingName.Should().Be("Wind Power Ltd");
    }

    [Fact]
    public async Task Search_PageSizeCappedAt50()
    {
        // Arrange
        var context = CreateDbContext();
        await SeedTestData(context);
        var service = CreateService(context);

        var query = new SupplierSearchQuery { PageSize = 100 };

        // Act
        var result = await service.SearchAsync(query);

        // Assert — PageSize should be capped at 50
        result.PageSize.Should().Be(50);
    }
}
