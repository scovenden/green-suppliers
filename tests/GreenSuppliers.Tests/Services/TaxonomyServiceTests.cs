using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class TaxonomyServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static TaxonomyService CreateService(GreenSuppliersDbContext context)
    {
        return new TaxonomyService(context);
    }

    // =========================================================================
    // Industries
    // =========================================================================

    [Fact]
    public async Task GetIndustries_ReturnsActiveIndustries_OrderedBySortOrder()
    {
        // Arrange
        var context = CreateDbContext();
        context.Industries.AddRange(
            new Industry { Id = Guid.NewGuid(), Name = "B Industry", Slug = "b-industry", SortOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Industry { Id = Guid.NewGuid(), Name = "A Industry", Slug = "a-industry", SortOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Industry { Id = Guid.NewGuid(), Name = "Inactive", Slug = "inactive", SortOrder = 0, IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetIndustriesAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("A Industry"); // SortOrder 1 first
        result[1].Name.Should().Be("B Industry");
    }

    [Fact]
    public async Task GetIndustries_ActiveOnlyFalse_ReturnsAll()
    {
        // Arrange
        var context = CreateDbContext();
        context.Industries.AddRange(
            new Industry { Id = Guid.NewGuid(), Name = "Active", Slug = "active", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Industry { Id = Guid.NewGuid(), Name = "Inactive", Slug = "inactive", IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetIndustriesAsync(activeOnly: false);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetIndustries_IncludesSupplierCount()
    {
        // Arrange
        var context = CreateDbContext();
        var industry = new Industry { Id = Guid.NewGuid(), Name = "Energy", Slug = "energy", IsActive = true, CreatedAt = DateTime.UtcNow };
        context.Industries.Add(industry);

        var org = new Organization { Id = Guid.NewGuid(), Name = "Org", CountryCode = "ZA", OrganizationType = OrganizationType.Supplier, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Organizations.Add(org);
        var profile = new SupplierProfile { Id = Guid.NewGuid(), OrganizationId = org.Id, Slug = "test", CountryCode = "ZA", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.SupplierProfiles.Add(profile);
        context.Set<SupplierIndustry>().Add(new SupplierIndustry { SupplierProfileId = profile.Id, IndustryId = industry.Id });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetIndustriesAsync();

        // Assert
        result.Should().HaveCount(1);
        result[0].SupplierCount.Should().Be(1);
    }

    [Fact]
    public async Task GetIndustryBySlug_Found_ReturnsDto()
    {
        // Arrange
        var context = CreateDbContext();
        var industry = new Industry { Id = Guid.NewGuid(), Name = "Renewable Energy", Slug = "renewable-energy", IsActive = true, CreatedAt = DateTime.UtcNow };
        context.Industries.Add(industry);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetIndustryBySlugAsync("renewable-energy");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Renewable Energy");
        result.Slug.Should().Be("renewable-energy");
    }

    [Fact]
    public async Task GetIndustryBySlug_NotFound_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetIndustryBySlugAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateIndustry_CreatesWithSlug()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.CreateIndustryAsync("Sustainable Construction", "Building green", null);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Sustainable Construction");
        result.Slug.Should().Be("sustainable-construction");
        result.Description.Should().Be("Building green");
        result.IsActive.Should().BeTrue();
        result.SupplierCount.Should().Be(0);

        var dbIndustry = await context.Industries.FirstAsync();
        dbIndustry.Name.Should().Be("Sustainable Construction");
    }

    [Fact]
    public async Task CreateIndustry_WithParentId_SetsParent()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var parentId = Guid.NewGuid();

        // Act
        var result = await service.CreateIndustryAsync("Sub-industry", null, parentId);

        // Assert
        result.ParentId.Should().Be(parentId);
    }

    [Fact]
    public async Task UpdateIndustry_ExistingIndustry_UpdatesFields()
    {
        // Arrange
        var context = CreateDbContext();
        var industry = new Industry { Id = Guid.NewGuid(), Name = "Old Name", Slug = "old-name", IsActive = true, CreatedAt = DateTime.UtcNow };
        context.Industries.Add(industry);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateIndustryAsync(industry.Id, "New Name", "New description");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.Slug.Should().Be("new-name");
        result.Description.Should().Be("New description");
    }

    [Fact]
    public async Task UpdateIndustry_NonExistent_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateIndustryAsync(Guid.NewGuid(), "Name", "Desc");

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // Certification Types
    // =========================================================================

    [Fact]
    public async Task GetCertTypes_ReturnsActiveOnly()
    {
        // Arrange
        var context = CreateDbContext();
        context.CertificationTypes.AddRange(
            new CertificationType { Id = Guid.NewGuid(), Name = "ISO 14001", Slug = "iso-14001", IsActive = true, CreatedAt = DateTime.UtcNow },
            new CertificationType { Id = Guid.NewGuid(), Name = "Old Cert", Slug = "old-cert", IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetCertTypesAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("ISO 14001");
    }

    [Fact]
    public async Task CreateCertType_CreatesWithSlug()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.CreateCertTypeAsync("B-Corp Certification", "Benefit corporation");

        // Assert
        result.Name.Should().Be("B-Corp Certification");
        result.Slug.Should().Be("b-corp-certification");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCertType_ExistingCertType_UpdatesFields()
    {
        // Arrange
        var context = CreateDbContext();
        var certType = new CertificationType { Id = Guid.NewGuid(), Name = "Old", Slug = "old", IsActive = true, CreatedAt = DateTime.UtcNow };
        context.CertificationTypes.Add(certType);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateCertTypeAsync(certType.Id, "Updated Cert", "Updated desc");

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated Cert");
        result.Slug.Should().Be("updated-cert");
    }

    [Fact]
    public async Task UpdateCertType_NonExistent_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateCertTypeAsync(Guid.NewGuid(), "Name", "Desc");

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // Service Tags
    // =========================================================================

    [Fact]
    public async Task GetServiceTags_ReturnsActiveOrderedByName()
    {
        // Arrange
        var context = CreateDbContext();
        context.ServiceTags.AddRange(
            new ServiceTag { Id = Guid.NewGuid(), Name = "Wind", Slug = "wind", IsActive = true, CreatedAt = DateTime.UtcNow },
            new ServiceTag { Id = Guid.NewGuid(), Name = "Solar", Slug = "solar", IsActive = true, CreatedAt = DateTime.UtcNow },
            new ServiceTag { Id = Guid.NewGuid(), Name = "Inactive Tag", Slug = "inactive", IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetServiceTagsAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Solar");
        result[1].Name.Should().Be("Wind");
    }

    [Fact]
    public async Task CreateServiceTag_CreatesWithSlug()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.CreateServiceTagAsync("Solar Panels");

        // Assert
        result.Name.Should().Be("Solar Panels");
        result.Slug.Should().Be("solar-panels");
        result.IsActive.Should().BeTrue();
    }

    // =========================================================================
    // Countries
    // =========================================================================

    [Fact]
    public async Task GetCountries_ReturnsActiveOrderedBySortOrder()
    {
        // Arrange
        var context = CreateDbContext();
        context.Countries.AddRange(
            new Country { Code = "KE", Name = "Kenya", Slug = "kenya", Region = "East Africa", IsActive = true, SortOrder = 2 },
            new Country { Code = "ZA", Name = "South Africa", Slug = "south-africa", Region = "Southern Africa", IsActive = true, SortOrder = 1 },
            new Country { Code = "XX", Name = "Inactive", Slug = "inactive", IsActive = false, SortOrder = 0 }
        );
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetCountriesAsync(activeOnly: true);

        // Assert
        result.Should().HaveCount(2);
        result[0].Code.Should().Be("ZA");
        result[1].Code.Should().Be("KE");
    }

    [Fact]
    public async Task GetCountries_IncludesSupplierCount()
    {
        // Arrange
        var context = CreateDbContext();
        context.Countries.Add(new Country { Code = "ZA", Name = "South Africa", Slug = "south-africa", IsActive = true });

        var org = new Organization { Id = Guid.NewGuid(), Name = "SA Org", CountryCode = "ZA", OrganizationType = OrganizationType.Supplier, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        context.Organizations.Add(org);
        context.SupplierProfiles.Add(new SupplierProfile
        {
            Id = Guid.NewGuid(), OrganizationId = org.Id, Slug = "sa-test", CountryCode = "ZA",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetCountriesAsync();

        // Assert
        result[0].SupplierCount.Should().Be(1);
    }

    [Fact]
    public async Task GetCountryByCode_Found_ReturnsDto()
    {
        // Arrange
        var context = CreateDbContext();
        context.Countries.Add(new Country { Code = "ZA", Name = "South Africa", Slug = "south-africa", Region = "Southern Africa", IsActive = true });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetCountryByCodeAsync("ZA");

        // Assert
        result.Should().NotBeNull();
        result!.Code.Should().Be("ZA");
        result.Name.Should().Be("South Africa");
        result.Region.Should().Be("Southern Africa");
    }

    [Fact]
    public async Task GetCountryByCode_NotFound_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetCountryByCodeAsync("XX");

        // Assert
        result.Should().BeNull();
    }
}
