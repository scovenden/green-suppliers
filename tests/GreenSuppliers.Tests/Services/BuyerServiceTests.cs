using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class BuyerServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static BuyerService CreateService(GreenSuppliersDbContext context)
    {
        var audit = new AuditService(context);
        return new BuyerService(context, audit);
    }

    private static async Task<(Guid BuyerUserId, Guid BuyerOrgId)> SeedBuyerAsync(GreenSuppliersDbContext context)
    {
        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Buyer Corp",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Buyer,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Organizations.Add(org);

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Email = "buyer@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Buyer",
            Role = UserRole.Buyer,
            EmailVerified = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (user.Id, org.Id);
    }

    private static async Task<SupplierProfile> SeedSupplierProfileAsync(GreenSuppliersDbContext context)
    {
        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Supplier Org",
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
            Slug = $"test-supplier-{Guid.NewGuid():N}",
            TradingName = "Test Supplier",
            ShortDescription = "A test supplier",
            CountryCode = "ZA",
            City = "Cape Town",
            IsPublished = true,
            EsgLevel = EsgLevel.Bronze,
            EsgScore = 25,
            VerificationStatus = VerificationStatus.Unverified,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();

        return profile;
    }

    [Fact]
    public async Task SaveSupplier_CreatesRecord()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        // Act
        var result = await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var saved = await context.SavedSuppliers.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.BuyerUserId.Should().Be(buyerUserId);
        saved.SupplierProfileId.Should().Be(profile.Id);
    }

    [Fact]
    public async Task SaveSupplier_DuplicatePrevented()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        // Save once
        await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);

        // Act — try to save again
        var result = await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        var count = await context.SavedSuppliers.CountAsync();
        count.Should().Be(1);
    }

    [Fact]
    public async Task UnsaveSupplier_RemovesRecord()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);
        var savedSupplier = await context.SavedSuppliers.FirstAsync();

        // Act
        var result = await service.UnsaveSupplierAsync(buyerUserId, savedSupplier.Id, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var count = await context.SavedSuppliers.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task UnsaveSupplier_WrongUser_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);
        var savedSupplier = await context.SavedSuppliers.FirstAsync();

        var otherUserId = Guid.NewGuid();

        // Act — try to unsave with a different user
        var result = await service.UnsaveSupplierAsync(otherUserId, savedSupplier.Id, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        var count = await context.SavedSuppliers.CountAsync();
        count.Should().Be(1); // record still exists
    }

    [Fact]
    public async Task GetSavedSuppliers_ReturnsOnlyUsersSuppliers()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);
        var profile1 = await SeedSupplierProfileAsync(context);
        var profile2 = await SeedSupplierProfileAsync(context);

        // Save both for this buyer
        await service.SaveSupplierAsync(buyerUserId, profile1.Id, CancellationToken.None);
        await service.SaveSupplierAsync(buyerUserId, profile2.Id, CancellationToken.None);

        // Save one for a different buyer (should not appear in results)
        var (otherBuyerId, _) = await SeedBuyerAsync(context);
        await service.SaveSupplierAsync(otherBuyerId, profile1.Id, CancellationToken.None);

        // Act
        var result = await service.GetSavedSuppliersAsync(buyerUserId, 1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
    }

    [Fact]
    public async Task GetBuyerLeads_ReturnsOnlyBuyerLeads()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, buyerOrgId) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        var now = DateTime.UtcNow;

        // Lead from this buyer
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            BuyerUserId = buyerUserId,
            BuyerOrganizationId = buyerOrgId,
            ContactName = "Buyer User",
            ContactEmail = "buyer@test.com",
            Message = "Interested in your services",
            Status = LeadStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Lead from another user (should not appear)
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            BuyerUserId = Guid.NewGuid(),
            ContactName = "Other User",
            ContactEmail = "other@test.com",
            Message = "Another inquiry",
            Status = LeadStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        });

        // Anonymous lead (no buyer user)
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            ContactName = "Anon User",
            ContactEmail = "anon@test.com",
            Message = "Anonymous inquiry",
            Status = LeadStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();

        // Act
        var result = await service.GetBuyerLeadsAsync(buyerUserId, 1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].ContactName.Should().Be("Buyer User");
        result.Total.Should().Be(1);
    }

    [Fact]
    public async Task GetDashboard_ReturnsCorrectCounts()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, buyerOrgId) = await SeedBuyerAsync(context);
        var profile1 = await SeedSupplierProfileAsync(context);
        var profile2 = await SeedSupplierProfileAsync(context);

        // Save 2 suppliers
        await service.SaveSupplierAsync(buyerUserId, profile1.Id, CancellationToken.None);
        await service.SaveSupplierAsync(buyerUserId, profile2.Id, CancellationToken.None);

        var now = DateTime.UtcNow;

        // Create 3 leads: 1 New, 1 Contacted, 1 Closed
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile1.Id,
            BuyerUserId = buyerUserId,
            BuyerOrganizationId = buyerOrgId,
            ContactName = "Buyer",
            ContactEmail = "buyer@test.com",
            Message = "Inquiry 1",
            Status = LeadStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile1.Id,
            BuyerUserId = buyerUserId,
            BuyerOrganizationId = buyerOrgId,
            ContactName = "Buyer",
            ContactEmail = "buyer@test.com",
            Message = "Inquiry 2",
            Status = LeadStatus.Contacted,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile2.Id,
            BuyerUserId = buyerUserId,
            BuyerOrganizationId = buyerOrgId,
            ContactName = "Buyer",
            ContactEmail = "buyer@test.com",
            Message = "Inquiry 3",
            Status = LeadStatus.Closed,
            CreatedAt = now,
            UpdatedAt = now
        });

        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDashboardAsync(buyerUserId, CancellationToken.None);

        // Assert
        result.SavedSupplierCount.Should().Be(2);
        result.InquirySentCount.Should().Be(3);
        result.InquiryRespondedCount.Should().Be(2); // Contacted + Closed (not New)
    }
}
