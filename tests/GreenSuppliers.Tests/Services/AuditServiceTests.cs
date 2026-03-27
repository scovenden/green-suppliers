using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Tests.Services;

public class AuditServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    [Fact]
    public async Task LogAsync_CreatesAuditEvent_WithAllFields()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new AuditService(context);
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        // Act
        await service.LogAsync(userId, "TestAction", "TestEntity", entityId,
            oldValues: "{\"field\":\"old\"}", newValues: "{\"field\":\"new\"}", ipAddress: "10.0.0.1");

        // Assert
        var audit = await context.AuditEvents.FirstOrDefaultAsync();
        audit.Should().NotBeNull();
        audit!.UserId.Should().Be(userId);
        audit.Action.Should().Be("TestAction");
        audit.EntityType.Should().Be("TestEntity");
        audit.EntityId.Should().Be(entityId);
        audit.OldValues.Should().Be("{\"field\":\"old\"}");
        audit.NewValues.Should().Be("{\"field\":\"new\"}");
        audit.IpAddress.Should().Be("10.0.0.1");
        audit.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task LogAsync_NullOptionalFields_CreatesEventWithNulls()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new AuditService(context);
        var entityId = Guid.NewGuid();

        // Act
        await service.LogAsync(null, "AnonymousAction", "Lead", entityId);

        // Assert
        var audit = await context.AuditEvents.FirstOrDefaultAsync();
        audit.Should().NotBeNull();
        audit!.UserId.Should().BeNull();
        audit.OldValues.Should().BeNull();
        audit.NewValues.Should().BeNull();
        audit.IpAddress.Should().BeNull();
    }

    [Fact]
    public async Task LogAsync_MultipleCalls_CreatesMultipleEvents()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new AuditService(context);

        // Act
        await service.LogAsync(null, "Action1", "Entity1", Guid.NewGuid());
        await service.LogAsync(null, "Action2", "Entity2", Guid.NewGuid());
        await service.LogAsync(null, "Action3", "Entity3", Guid.NewGuid());

        // Assert
        var count = await context.AuditEvents.CountAsync();
        count.Should().Be(3);
    }

    [Fact]
    public async Task LogAsync_GeneratesUniqueIds()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new AuditService(context);

        // Act
        await service.LogAsync(null, "Action1", "Entity", Guid.NewGuid());
        await service.LogAsync(null, "Action2", "Entity", Guid.NewGuid());

        // Assert
        var events = await context.AuditEvents.ToListAsync();
        events.Select(e => e.Id).Should().OnlyHaveUniqueItems();
    }
}
