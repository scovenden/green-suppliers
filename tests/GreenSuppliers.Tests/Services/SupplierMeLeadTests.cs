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
}
