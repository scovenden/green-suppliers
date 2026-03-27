using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class DocumentServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static DocumentService CreateService(GreenSuppliersDbContext context)
    {
        return new DocumentService(context);
    }

    private static async Task<Guid> SeedSupplierProfileAsync(GreenSuppliersDbContext context)
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
            CountryCode = "ZA",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();
        return profile.Id;
    }

    [Fact]
    public async Task CreateAsync_CreatesDocument_WithAllFields()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profileId = await SeedSupplierProfileAsync(context);
        var userId = Guid.NewGuid();

        // Act
        var result = await service.CreateAsync(
            profileId,
            "certificate.pdf",
            "https://storage.azure.com/documents/certificate.pdf",
            "application/pdf",
            1024000,
            "certification",
            userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.SupplierProfileId.Should().Be(profileId);
        result.FileName.Should().Be("certificate.pdf");
        result.BlobUrl.Should().Be("https://storage.azure.com/documents/certificate.pdf");
        result.ContentType.Should().Be("application/pdf");
        result.FileSizeBytes.Should().Be(1024000);
        result.DocumentType.Should().Be("certification");
        result.UploadedByUserId.Should().Be(userId);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var dbDoc = await context.Documents.FirstAsync();
        dbDoc.FileName.Should().Be("certificate.pdf");
    }

    [Fact]
    public async Task CreateAsync_NullUserId_IsAllowed()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profileId = await SeedSupplierProfileAsync(context);

        // Act
        var result = await service.CreateAsync(profileId, "report.pdf", "https://blob/report.pdf",
            "application/pdf", 512000, "report", null);

        // Assert
        result.UploadedByUserId.Should().BeNull();
    }

    [Fact]
    public async Task GetBySupplier_ReturnsDocumentsForProfile()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profileId = await SeedSupplierProfileAsync(context);
        var otherProfileId = await SeedSupplierProfileAsync(context);

        await service.CreateAsync(profileId, "doc1.pdf", "url1", "application/pdf", 100, "cert", null);
        await service.CreateAsync(profileId, "doc2.pdf", "url2", "application/pdf", 200, "cert", null);
        await service.CreateAsync(otherProfileId, "other.pdf", "url3", "application/pdf", 300, "cert", null);

        // Act
        var result = await service.GetBySupplierAsync(profileId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(d => d.SupplierProfileId == profileId);
    }

    [Fact]
    public async Task GetBySupplier_OrderedByCreatedAtDescending()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profileId = await SeedSupplierProfileAsync(context);

        // Create docs with slightly different timestamps
        context.Documents.Add(new Document
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            FileName = "oldest.pdf",
            BlobUrl = "url1",
            ContentType = "application/pdf",
            FileSizeBytes = 100,
            DocumentType = "cert",
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        });
        context.Documents.Add(new Document
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            FileName = "newest.pdf",
            BlobUrl = "url2",
            ContentType = "application/pdf",
            FileSizeBytes = 200,
            DocumentType = "cert",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetBySupplierAsync(profileId);

        // Assert
        result[0].FileName.Should().Be("newest.pdf");
        result[1].FileName.Should().Be("oldest.pdf");
    }

    [Fact]
    public async Task GetBySupplier_NoDocuments_ReturnsEmptyList()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profileId = await SeedSupplierProfileAsync(context);

        // Act
        var result = await service.GetBySupplierAsync(profileId);

        // Assert
        result.Should().BeEmpty();
    }
}
