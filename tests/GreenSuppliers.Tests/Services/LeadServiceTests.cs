using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GreenSuppliers.Tests.Services;

public class LeadServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static LeadService CreateService(GreenSuppliersDbContext context)
    {
        var audit = new AuditService(context);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Notifications:AdminEmail"] = "admin@test.com"
            })
            .Build();
        return new LeadService(context, audit, config);
    }

    private static async Task<SupplierProfile> SeedSupplierProfileAsync(GreenSuppliersDbContext context)
    {
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Supplier Org",
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
            Slug = "test-supplier",
            CountryCode = "ZA",
            IsPublished = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.SupplierProfiles.Add(profile);
        await context.SaveChangesAsync();

        return profile;
    }

    [Fact]
    public async Task CreateLead_ValidRequest_ReturnsLead()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profile = await SeedSupplierProfileAsync(context);

        var request = new LeadRequest
        {
            SupplierProfileId = profile.Id,
            ContactName = "John Doe",
            ContactEmail = "john@example.com",
            ContactPhone = "+27821234567",
            CompanyName = "Buyer Corp",
            Message = "We are interested in your sustainable packaging."
        };

        // Act
        var result = await service.CreateLeadAsync(request, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result.ContactName.Should().Be("John Doe");
        result.ContactEmail.Should().Be("john@example.com");
        result.ContactPhone.Should().Be("+27821234567");
        result.CompanyName.Should().Be("Buyer Corp");
        result.Message.Should().Be("We are interested in your sustainable packaging.");
        result.Status.Should().Be("New");
        result.LeadType.Should().Be("inquiry");
        result.SupplierProfileId.Should().Be(profile.Id);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateLead_AuditsSubmission()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profile = await SeedSupplierProfileAsync(context);

        var request = new LeadRequest
        {
            SupplierProfileId = profile.Id,
            ContactName = "Jane Doe",
            ContactEmail = "jane@example.com",
            Message = "Inquiry about your services."
        };

        // Act
        await service.CreateLeadAsync(request, "10.0.0.1");

        // Assert
        var auditEvent = await context.AuditEvents.FirstOrDefaultAsync();
        auditEvent.Should().NotBeNull();
        auditEvent!.Action.Should().Be("LeadCreated");
        auditEvent.EntityType.Should().Be("Lead");
        auditEvent.IpAddress.Should().Be("10.0.0.1");
    }

    [Fact]
    public async Task CreateLead_QueuesEmailNotification()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profile = await SeedSupplierProfileAsync(context);

        var request = new LeadRequest
        {
            SupplierProfileId = profile.Id,
            ContactName = "Bob Smith",
            ContactEmail = "bob@example.com",
            Message = "Please contact me."
        };

        // Act
        await service.CreateLeadAsync(request, "192.168.1.1");

        // Assert
        var emailItem = await context.EmailQueue.FirstOrDefaultAsync();
        emailItem.Should().NotBeNull();
        emailItem!.TemplateType.Should().Be("lead_notification");
        emailItem.Status.Should().Be("pending");
    }

    [Fact]
    public async Task CreateGetListed_CreatesLeadWithGetListedType()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        var request = new GetListedRequest
        {
            CompanyName = "Green Energy Co",
            ContactName = "Alice Green",
            ContactEmail = "alice@greenenergy.co.za",
            ContactPhone = "+27119876543",
            Website = "https://greenenergy.co.za",
            Country = "ZA",
            City = "Johannesburg",
            Description = "We provide solar panel installations across South Africa.",
            Certifications = "ISO 14001, B-Corp"
        };

        // Act
        var result = await service.CreateGetListedAsync(request, "203.0.113.1");

        // Assert
        result.Should().NotBeNull();
        result.LeadType.Should().Be("get_listed");
        result.ContactName.Should().Be("Alice Green");
        result.ContactEmail.Should().Be("alice@greenenergy.co.za");
        result.CompanyName.Should().Be("Green Energy Co");
        result.Status.Should().Be("New");
    }

    [Fact]
    public async Task UpdateStatus_ChangesLeadStatus()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profile = await SeedSupplierProfileAsync(context);

        var request = new LeadRequest
        {
            SupplierProfileId = profile.Id,
            ContactName = "Test User",
            ContactEmail = "test@example.com",
            Message = "Test message"
        };
        var lead = await service.CreateLeadAsync(request, "127.0.0.1");
        var adminUserId = Guid.NewGuid();

        // Act
        var result = await service.UpdateStatusAsync(lead.Id, "Contacted", adminUserId);

        // Assert
        result.Should().BeTrue();
        var updatedLead = await context.Leads.FindAsync(lead.Id);
        updatedLead!.Status.Should().Be(LeadStatus.Contacted);
    }

    [Fact]
    public async Task GetAll_FiltersByStatus()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);
        var profile = await SeedSupplierProfileAsync(context);

        // Create 3 leads
        for (int i = 0; i < 3; i++)
        {
            var req = new LeadRequest
            {
                SupplierProfileId = profile.Id,
                ContactName = $"User {i}",
                ContactEmail = $"user{i}@example.com",
                Message = $"Message {i}"
            };
            await service.CreateLeadAsync(req, "127.0.0.1");
        }

        // Mark first as Contacted
        var allLeads = await context.Leads.ToListAsync();
        allLeads[0].Status = LeadStatus.Contacted;
        await context.SaveChangesAsync();

        // Act — filter by "New" status
        var result = await service.GetAllAsync(1, 10, "New");

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(l => l.Status == "New");
        result.Total.Should().Be(2);
    }
}
