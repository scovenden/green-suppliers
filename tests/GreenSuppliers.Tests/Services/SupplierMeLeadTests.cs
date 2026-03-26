using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace GreenSuppliers.Tests.Services;

public class SupplierMeLeadTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static SupplierMeService CreateService(GreenSuppliersDbContext context)
    {
        var esgScoring = new EsgScoringService();
        var verification = new VerificationService();
        var audit = new AuditService(context);
        var logger = Mock.Of<ILogger<SupplierMeService>>();
        return new SupplierMeService(context, esgScoring, verification, audit, logger);
    }

    private static async Task<(Guid OrgId, Guid ProfileId)> SeedSupplierAsync(GreenSuppliersDbContext context)
    {
        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Eco Solutions (Pty) Ltd",
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
            Slug = $"eco-solutions-{Guid.NewGuid():N}",
            TradingName = "Eco Solutions",
            CountryCode = "ZA",
            IsPublished = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();
        return (org.Id, profile.Id);
    }

    private static async Task SeedLeadsAsync(GreenSuppliersDbContext context, Guid profileId, int count, LeadStatus status = LeadStatus.New)
    {
        var now = DateTime.UtcNow;
        for (int i = 0; i < count; i++)
        {
            context.Leads.Add(new Lead
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = profileId,
                ContactName = $"Contact {i}",
                ContactEmail = $"contact{i}@example.com",
                Message = $"Message {i}",
                Status = status,
                CreatedAt = now.AddMinutes(-i), // varied timestamps for ordering
                UpdatedAt = now.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetLeads_ReturnsOnlySupplierLeads()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var (_, otherProfileId) = await SeedSupplierAsync(context);

        await SeedLeadsAsync(context, profileId, 3);
        await SeedLeadsAsync(context, otherProfileId, 2); // leads for a different supplier

        // Act
        var result = await service.GetLeadsAsync(orgId, 1, 20, null, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Total.Should().Be(3);
        result.Items.Should().OnlyContain(l => l.SupplierProfileId == profileId);
    }

    [Fact]
    public async Task GetLeadDetail_WrongSupplier_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var (otherOrgId, otherProfileId) = await SeedSupplierAsync(context);

        await SeedLeadsAsync(context, otherProfileId, 1);
        var leadId = (await context.Leads.FirstAsync(l => l.SupplierProfileId == otherProfileId)).Id;

        // Act — try to access other supplier's lead
        var result = await service.GetLeadDetailAsync(orgId, leadId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateLeadStatus_ValidTransition_Succeeds()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.New);
        var leadId = (await context.Leads.FirstAsync()).Id;

        // Act — New -> Contacted
        var result = await service.UpdateLeadStatusAsync(orgId, leadId, "Contacted", userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updatedLead = await context.Leads.FindAsync(leadId);
        updatedLead!.Status.Should().Be(LeadStatus.Contacted);

        // Verify audit log was written
        var auditEvent = await context.AuditEvents
            .FirstOrDefaultAsync(a => a.Action == "SupplierLeadStatusChanged" && a.EntityId == leadId);
        auditEvent.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLeadStatus_InvalidTransition_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.New);
        var leadId = (await context.Leads.FirstAsync()).Id;

        // Act — New -> Closed (skipping Contacted) should fail
        var result = await service.UpdateLeadStatusAsync(orgId, leadId, "Closed", userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        var lead = await context.Leads.FindAsync(leadId);
        lead!.Status.Should().Be(LeadStatus.New); // unchanged
    }

    // =========================================================================
    // GetLeadsAsync — status filter
    // =========================================================================

    [Fact]
    public async Task GetLeads_WithStatusFilter_ReturnsOnlyMatchingLeads()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        await SeedLeadsAsync(context, profileId, 3, LeadStatus.New);
        await SeedLeadsAsync(context, profileId, 2, LeadStatus.Contacted);

        // Act — filter by Contacted
        var result = await service.GetLeadsAsync(orgId, 1, 20, "Contacted", CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(2);
        result.Items.Should().OnlyContain(l => l.Status == "Contacted");
    }

    [Fact]
    public async Task GetLeads_WithInvalidStatusFilter_ReturnsAllLeads()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        await SeedLeadsAsync(context, profileId, 3, LeadStatus.New);

        // Act — invalid status string should be ignored, return all
        var result = await service.GetLeadsAsync(orgId, 1, 20, "NotAValidStatus", CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Total.Should().Be(3);
    }

    [Fact]
    public async Task GetLeads_WithNullStatusFilter_ReturnsAllLeads()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        await SeedLeadsAsync(context, profileId, 2, LeadStatus.New);
        await SeedLeadsAsync(context, profileId, 1, LeadStatus.Contacted);

        // Act
        var result = await service.GetLeadsAsync(orgId, 1, 20, null, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Total.Should().Be(3);
    }

    [Fact]
    public async Task GetLeads_NoProfile_ReturnsEmptyResult()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var fakeOrgId = Guid.NewGuid();

        // Act — org has no profile
        var result = await service.GetLeadsAsync(fakeOrgId, 1, 20, null, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task GetLeads_PaginationRespectsPageSize()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        await SeedLeadsAsync(context, profileId, 5);

        // Act
        var page1 = await service.GetLeadsAsync(orgId, 1, 2, null, CancellationToken.None);
        var page2 = await service.GetLeadsAsync(orgId, 2, 2, null, CancellationToken.None);

        // Assert
        page1.Items.Should().HaveCount(2);
        page1.Total.Should().Be(5);
        page2.Items.Should().HaveCount(2);
        page2.Total.Should().Be(5);
    }

    // =========================================================================
    // GetLeadDetailAsync — happy path and edge cases
    // =========================================================================

    [Fact]
    public async Task GetLeadDetail_OwnLead_ReturnsCorrectDto()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        await SeedLeadsAsync(context, profileId, 1);
        var lead = await context.Leads.FirstAsync(l => l.SupplierProfileId == profileId);

        // Act
        var result = await service.GetLeadDetailAsync(orgId, lead.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(lead.Id);
        result.SupplierProfileId.Should().Be(profileId);
        result.ContactName.Should().Be("Contact 0");
        result.ContactEmail.Should().Be("contact0@example.com");
    }

    [Fact]
    public async Task GetLeadDetail_NonExistentLead_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _) = await SeedSupplierAsync(context);

        // Act
        var result = await service.GetLeadDetailAsync(orgId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLeadDetail_NoProfile_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act — org with no profile
        var result = await service.GetLeadDetailAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // UpdateLeadStatusAsync — additional transitions and edge cases
    // =========================================================================

    [Fact]
    public async Task UpdateLeadStatus_ContactedToClosed_Succeeds()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.Contacted);
        var leadId = (await context.Leads.FirstAsync()).Id;

        // Act — Contacted -> Closed is valid
        var result = await service.UpdateLeadStatusAsync(orgId, leadId, "Closed", userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updatedLead = await context.Leads.FindAsync(leadId);
        updatedLead!.Status.Should().Be(LeadStatus.Closed);
    }

    [Fact]
    public async Task UpdateLeadStatus_ClosedToAnything_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.Closed);
        var leadId = (await context.Leads.FirstAsync()).Id;

        // Act — Closed -> New is not a valid transition
        var result = await service.UpdateLeadStatusAsync(orgId, leadId, "New", userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        var lead = await context.Leads.FindAsync(leadId);
        lead!.Status.Should().Be(LeadStatus.Closed); // unchanged
    }

    [Fact]
    public async Task UpdateLeadStatus_InvalidStatusString_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.New);
        var leadId = (await context.Leads.FirstAsync()).Id;

        // Act — "InvalidStatus" is not a valid LeadStatus enum value
        var result = await service.UpdateLeadStatusAsync(orgId, leadId, "InvalidStatus", userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLeadStatus_WrongSupplier_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var (otherOrgId, _) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.New);
        var leadId = (await context.Leads.FirstAsync(l => l.SupplierProfileId == profileId)).Id;

        // Act — try to update using a different supplier's org ID
        var result = await service.UpdateLeadStatusAsync(otherOrgId, leadId, "Contacted", userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        var lead = await context.Leads.FindAsync(leadId);
        lead!.Status.Should().Be(LeadStatus.New); // unchanged
    }

    [Fact]
    public async Task UpdateLeadStatus_NonExistentLead_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, _) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        // Act
        var result = await service.UpdateLeadStatusAsync(orgId, Guid.NewGuid(), "Contacted", userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLeadStatus_NoProfile_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var userId = Guid.NewGuid();

        // Act — org with no profile
        var result = await service.UpdateLeadStatusAsync(Guid.NewGuid(), Guid.NewGuid(), "Contacted", userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLeadStatus_CaseInsensitiveStatusString_Succeeds()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.New);
        var leadId = (await context.Leads.FirstAsync()).Id;

        // Act — "contacted" instead of "Contacted"
        var result = await service.UpdateLeadStatusAsync(orgId, leadId, "contacted", userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var lead = await context.Leads.FindAsync(leadId);
        lead!.Status.Should().Be(LeadStatus.Contacted);
    }

    [Fact]
    public async Task UpdateLeadStatus_ValidTransition_UpdatesTimestamp()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);
        var userId = Guid.NewGuid();

        await SeedLeadsAsync(context, profileId, 1, LeadStatus.New);
        var lead = await context.Leads.FirstAsync();
        var originalUpdatedAt = lead.UpdatedAt;

        // Act
        var result = await service.UpdateLeadStatusAsync(orgId, lead.Id, "Contacted", userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var updatedLead = await context.Leads.FindAsync(lead.Id);
        updatedLead!.UpdatedAt.Should().BeOnOrAfter(originalUpdatedAt);
    }

    // =========================================================================
    // GetLeadsAsync — leads ordered by CreatedAt descending
    // =========================================================================

    [Fact]
    public async Task GetLeads_ReturnsLeadsInDescendingCreatedAtOrder()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var (orgId, profileId) = await SeedSupplierAsync(context);

        var now = DateTime.UtcNow;
        // Add leads with specific timestamps
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            ContactName = "Oldest",
            ContactEmail = "oldest@test.com",
            Message = "Old inquiry",
            Status = LeadStatus.New,
            CreatedAt = now.AddHours(-2),
            UpdatedAt = now.AddHours(-2)
        });
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            ContactName = "Newest",
            ContactEmail = "newest@test.com",
            Message = "New inquiry",
            Status = LeadStatus.New,
            CreatedAt = now,
            UpdatedAt = now
        });
        context.Leads.Add(new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = profileId,
            ContactName = "Middle",
            ContactEmail = "middle@test.com",
            Message = "Middle inquiry",
            Status = LeadStatus.New,
            CreatedAt = now.AddHours(-1),
            UpdatedAt = now.AddHours(-1)
        });
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetLeadsAsync(orgId, 1, 20, null, CancellationToken.None);

        // Assert — newest first
        result.Items.Should().HaveCount(3);
        result.Items[0].ContactName.Should().Be("Newest");
        result.Items[1].ContactName.Should().Be("Middle");
        result.Items[2].ContactName.Should().Be("Oldest");
    }
}
