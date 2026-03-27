using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class ContentServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static ContentService CreateService(GreenSuppliersDbContext context)
    {
        return new ContentService(context);
    }

    [Fact]
    public async Task GetBySlug_PublishedPage_ReturnsDto()
    {
        // Arrange
        var context = CreateDbContext();
        var now = DateTime.UtcNow;
        context.ContentPages.Add(new ContentPage
        {
            Id = Guid.NewGuid(),
            Slug = "about-green-suppliers",
            Title = "About Green Suppliers",
            Body = "<p>We connect buyers with green suppliers.</p>",
            PageType = "pillar",
            MetaTitle = "About Us",
            MetaDesc = "Learn about Green Suppliers",
            IsPublished = true,
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetBySlugAsync("about-green-suppliers");

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("About Green Suppliers");
        result.Slug.Should().Be("about-green-suppliers");
        result.Body.Should().Contain("green suppliers");
        result.PageType.Should().Be("pillar");
        result.MetaTitle.Should().Be("About Us");
        result.MetaDesc.Should().Be("Learn about Green Suppliers");
        result.IsPublished.Should().BeTrue();
    }

    [Fact]
    public async Task GetBySlug_UnpublishedPage_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        context.ContentPages.Add(new ContentPage
        {
            Id = Guid.NewGuid(),
            Slug = "draft-page",
            Title = "Draft",
            Body = "Draft content",
            PageType = "guide",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetBySlugAsync("draft-page");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBySlug_NonExistent_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetBySlugAsync("does-not-exist");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAll_ReturnsPaginatedResults()
    {
        // Arrange
        var context = CreateDbContext();
        for (int i = 0; i < 5; i++)
        {
            context.ContentPages.Add(new ContentPage
            {
                Id = Guid.NewGuid(),
                Slug = $"page-{i}",
                Title = $"Page {i}",
                Body = $"Content {i}",
                PageType = "guide",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(1, 3);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Total.Should().Be(5);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(3);
    }

    [Fact]
    public async Task GetAll_Page2_ReturnsRemainingItems()
    {
        // Arrange
        var context = CreateDbContext();
        for (int i = 0; i < 5; i++)
        {
            context.ContentPages.Add(new ContentPage
            {
                Id = Guid.NewGuid(),
                Slug = $"page-{i}",
                Title = $"Page {i}",
                Body = $"Content {i}",
                PageType = "guide",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(2, 3);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
    }

    [Fact]
    public async Task CreateAsync_CreatesUnpublishedPage()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.CreateAsync("Guide Title", "guide-title", "<p>Guide body</p>",
            "guide", "SEO Title", "SEO Description");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Guide Title");
        result.Slug.Should().Be("guide-title");
        result.Body.Should().Be("<p>Guide body</p>");
        result.PageType.Should().Be("guide");
        result.MetaTitle.Should().Be("SEO Title");
        result.MetaDesc.Should().Be("SEO Description");
        result.IsPublished.Should().BeFalse();
        result.PublishedAt.Should().BeNull();

        var dbPage = await context.ContentPages.FirstAsync();
        dbPage.Title.Should().Be("Guide Title");
    }

    [Fact]
    public async Task UpdateAsync_ExistingPage_UpdatesFields()
    {
        // Arrange
        var context = CreateDbContext();
        var page = new ContentPage
        {
            Id = Guid.NewGuid(),
            Slug = "original-slug",
            Title = "Original Title",
            Body = "Original body",
            PageType = "guide",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.ContentPages.Add(page);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateAsync(page.Id, "Updated Title", "updated-slug",
            "Updated body", "New Meta", "New Desc", isPublished: false);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Slug.Should().Be("updated-slug");
        result.Body.Should().Be("Updated body");
        result.MetaTitle.Should().Be("New Meta");
        result.MetaDesc.Should().Be("New Desc");
    }

    [Fact]
    public async Task UpdateAsync_SetPublished_SetsPublishedAt()
    {
        // Arrange
        var context = CreateDbContext();
        var page = new ContentPage
        {
            Id = Guid.NewGuid(),
            Slug = "to-publish",
            Title = "To Publish",
            Body = "Content",
            PageType = "guide",
            IsPublished = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.ContentPages.Add(page);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateAsync(page.Id, "To Publish", null, "Content",
            null, null, isPublished: true);

        // Assert
        result!.IsPublished.Should().BeTrue();
        result.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_Unpublish_ClearsPublishedFlag()
    {
        // Arrange
        var context = CreateDbContext();
        var now = DateTime.UtcNow;
        var page = new ContentPage
        {
            Id = Guid.NewGuid(),
            Slug = "published-page",
            Title = "Published",
            Body = "Content",
            PageType = "guide",
            IsPublished = true,
            PublishedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.ContentPages.Add(page);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateAsync(page.Id, "Published", null, "Content",
            null, null, isPublished: false);

        // Assert
        result!.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_NonExistent_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.UpdateAsync(Guid.NewGuid(), "Title", null, "Body",
            null, null, isPublished: false);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_NullSlug_PreservesExistingSlug()
    {
        // Arrange
        var context = CreateDbContext();
        var page = new ContentPage
        {
            Id = Guid.NewGuid(),
            Slug = "original-slug",
            Title = "Title",
            Body = "Body",
            PageType = "guide",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.ContentPages.Add(page);
        await context.SaveChangesAsync();
        var service = CreateService(context);

        // Act — pass null slug
        var result = await service.UpdateAsync(page.Id, "New Title", null, "New Body",
            null, null, isPublished: false);

        // Assert — slug should remain unchanged
        result!.Slug.Should().Be("original-slug");
    }
}
