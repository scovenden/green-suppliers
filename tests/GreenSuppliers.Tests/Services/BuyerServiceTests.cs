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

    // =========================================================================
    // SaveSupplierAsync — edge cases
    // =========================================================================

    [Fact]
    public async Task SaveSupplier_NonExistentProfile_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        // Act — try to save a profile ID that does not exist
        var result = await service.SaveSupplierAsync(buyerUserId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        var count = await context.SavedSuppliers.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task SaveSupplier_UnpublishedProfile_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Unpublished Supplier Org",
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
            Slug = $"unpublished-{Guid.NewGuid():N}",
            TradingName = "Unpublished Supplier",
            CountryCode = "ZA",
            IsPublished = false, // Not published
            CreatedAt = now,
            UpdatedAt = now
        };
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveSupplier_DeletedProfile_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Deleted Supplier Org",
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
            Slug = $"deleted-{Guid.NewGuid():N}",
            TradingName = "Deleted Supplier",
            CountryCode = "ZA",
            IsPublished = true,
            IsDeleted = true, // Soft-deleted
            DeletedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SaveSupplier_WritesAuditLog()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        // Act
        await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);

        // Assert
        var auditEvent = await context.AuditEvents
            .FirstOrDefaultAsync(a => a.Action == "SupplierSaved" && a.UserId == buyerUserId);
        auditEvent.Should().NotBeNull();
        auditEvent!.EntityType.Should().Be("SavedSupplier");
    }

    // =========================================================================
    // UnsaveSupplierAsync — edge cases
    // =========================================================================

    [Fact]
    public async Task UnsaveSupplier_NonExistentId_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        // Act — try to unsave a record that does not exist
        var result = await service.UnsaveSupplierAsync(buyerUserId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnsaveSupplier_WritesAuditLog()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);
        var savedSupplier = await context.SavedSuppliers.FirstAsync();

        // Act
        await service.UnsaveSupplierAsync(buyerUserId, savedSupplier.Id, CancellationToken.None);

        // Assert
        var auditEvent = await context.AuditEvents
            .FirstOrDefaultAsync(a => a.Action == "SupplierUnsaved" && a.UserId == buyerUserId);
        auditEvent.Should().NotBeNull();
        auditEvent!.EntityType.Should().Be("SavedSupplier");
    }

    // =========================================================================
    // GetSavedSuppliersAsync — pagination and empty state
    // =========================================================================

    [Fact]
    public async Task GetSavedSuppliers_NoSavedSuppliers_ReturnsEmptyResult()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        // Act
        var result = await service.GetSavedSuppliersAsync(buyerUserId, 1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task GetSavedSuppliers_PaginationRespectsPageSize()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        // Save 3 suppliers
        for (int i = 0; i < 3; i++)
        {
            var profile = await SeedSupplierProfileAsync(context);
            await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);
        }

        // Act — request page 1 with pageSize 2
        var page1 = await service.GetSavedSuppliersAsync(buyerUserId, 1, 2, CancellationToken.None);
        var page2 = await service.GetSavedSuppliersAsync(buyerUserId, 2, 2, CancellationToken.None);

        // Assert
        page1.Items.Should().HaveCount(2);
        page1.Total.Should().Be(3);
        page1.Page.Should().Be(1);
        page1.PageSize.Should().Be(2);

        page2.Items.Should().HaveCount(1);
        page2.Total.Should().Be(3);
        page2.Page.Should().Be(2);
    }

    // =========================================================================
    // GetBuyerLeadsAsync — pagination and DTO mapping
    // =========================================================================

    [Fact]
    public async Task GetBuyerLeads_NoLeads_ReturnsEmptyResult()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        // Act
        var result = await service.GetBuyerLeadsAsync(buyerUserId, 1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task GetBuyerLeads_MapsLeadFieldsCorrectly()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, buyerOrgId) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        var now = DateTime.UtcNow;
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            BuyerUserId = buyerUserId,
            BuyerOrganizationId = buyerOrgId,
            ContactName = "John Smith",
            ContactEmail = "john@corp.com",
            ContactPhone = "+27821234567",
            CompanyName = "Corp Ltd",
            Message = "Need green packaging",
            Status = LeadStatus.Contacted,
            LeadType = "inquiry",
            IpAddress = "192.168.1.1",
            CreatedAt = now,
            UpdatedAt = now
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetBuyerLeadsAsync(buyerUserId, 1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var lead = result.Items[0];
        lead.ContactName.Should().Be("John Smith");
        lead.ContactEmail.Should().Be("john@corp.com");
        lead.ContactPhone.Should().Be("+27821234567");
        lead.CompanyName.Should().Be("Corp Ltd");
        lead.Message.Should().Be("Need green packaging");
        lead.Status.Should().Be("Contacted");
        lead.LeadType.Should().Be("inquiry");
        lead.SupplierProfileId.Should().Be(profile.Id);
        lead.BuyerUserId.Should().Be(buyerUserId);
        lead.BuyerOrganizationId.Should().Be(buyerOrgId);
    }

    [Fact]
    public async Task GetBuyerLeads_PaginationRespectsPageSize()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, buyerOrgId) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        var now = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            context.Leads.Add(new Lead
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = profile.Id,
                BuyerUserId = buyerUserId,
                BuyerOrganizationId = buyerOrgId,
                ContactName = $"Contact {i}",
                ContactEmail = $"contact{i}@test.com",
                Message = $"Inquiry {i}",
                Status = LeadStatus.New,
                CreatedAt = now.AddMinutes(-i),
                UpdatedAt = now.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        // Act
        var page1 = await service.GetBuyerLeadsAsync(buyerUserId, 1, 2, CancellationToken.None);
        var page2 = await service.GetBuyerLeadsAsync(buyerUserId, 2, 2, CancellationToken.None);

        // Assert
        page1.Items.Should().HaveCount(2);
        page1.Total.Should().Be(5);
        page2.Items.Should().HaveCount(2);
        page2.Total.Should().Be(5);
    }

    // =========================================================================
    // GetDashboardAsync — edge cases
    // =========================================================================

    [Fact]
    public async Task GetDashboard_NoData_ReturnsAllZeros()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, _) = await SeedBuyerAsync(context);

        // Act — no saved suppliers, no leads
        var result = await service.GetDashboardAsync(buyerUserId, CancellationToken.None);

        // Assert
        result.SavedSupplierCount.Should().Be(0);
        result.InquirySentCount.Should().Be(0);
        result.InquiryRespondedCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboard_AllLeadsNew_InquiryRespondedCountIsZero()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, buyerOrgId) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        var now = DateTime.UtcNow;
        for (int i = 0; i < 3; i++)
        {
            context.Leads.Add(new Lead
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = profile.Id,
                BuyerUserId = buyerUserId,
                BuyerOrganizationId = buyerOrgId,
                ContactName = "Buyer",
                ContactEmail = "buyer@test.com",
                Message = $"Inquiry {i}",
                Status = LeadStatus.New, // All New — none responded
                CreatedAt = now,
                UpdatedAt = now
            });
        }
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDashboardAsync(buyerUserId, CancellationToken.None);

        // Assert
        result.InquirySentCount.Should().Be(3);
        result.InquiryRespondedCount.Should().Be(0);
    }

    [Fact]
    public async Task GetDashboard_DoesNotCountOtherUserData()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (buyerUserId, buyerOrgId) = await SeedBuyerAsync(context);
        var (otherBuyerId, otherBuyerOrgId) = await SeedBuyerAsync(context);
        var profile = await SeedSupplierProfileAsync(context);

        // Save supplier for both users
        await service.SaveSupplierAsync(buyerUserId, profile.Id, CancellationToken.None);
        await service.SaveSupplierAsync(otherBuyerId, profile.Id, CancellationToken.None);

        var now = DateTime.UtcNow;
        // Lead for our user
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            BuyerUserId = buyerUserId,
            BuyerOrganizationId = buyerOrgId,
            ContactName = "Our Buyer",
            ContactEmail = "ours@test.com",
            Message = "Our inquiry",
            Status = LeadStatus.Contacted,
            CreatedAt = now,
            UpdatedAt = now
        });
        // Lead for another user (should not be counted)
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profile.Id,
            BuyerUserId = otherBuyerId,
            BuyerOrganizationId = otherBuyerOrgId,
            ContactName = "Other Buyer",
            ContactEmail = "other@test.com",
            Message = "Other inquiry",
            Status = LeadStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetDashboardAsync(buyerUserId, CancellationToken.None);

        // Assert
        result.SavedSupplierCount.Should().Be(1);
        result.InquirySentCount.Should().Be(1);
        result.InquiryRespondedCount.Should().Be(1); // Contacted counts as responded
    }
}
