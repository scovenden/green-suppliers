using FluentAssertions;
using GreenSuppliers.Api.Helpers;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;

namespace GreenSuppliers.Tests.Helpers;

public class LeadMapperTests
{
    [Fact]
    public void MapToDto_MapsAllFieldsCorrectly()
    {
        // Arrange
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = Guid.NewGuid(),
            BuyerOrganizationId = Guid.NewGuid(),
            BuyerUserId = Guid.NewGuid(),
            ContactName = "John Doe",
            ContactEmail = "john@example.com",
            ContactPhone = "+27821234567",
            CompanyName = "Buyer Corp",
            Message = "Interested in your green solutions.",
            Status = LeadStatus.Contacted,
            LeadType = "inquiry",
            IpAddress = "192.168.1.1",
            CreatedAt = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 3, 2, 14, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var dto = LeadMapper.MapToDto(lead);

        // Assert
        dto.Id.Should().Be(lead.Id);
        dto.SupplierProfileId.Should().Be(lead.SupplierProfileId);
        dto.BuyerOrganizationId.Should().Be(lead.BuyerOrganizationId);
        dto.BuyerUserId.Should().Be(lead.BuyerUserId);
        dto.ContactName.Should().Be("John Doe");
        dto.ContactEmail.Should().Be("john@example.com");
        dto.ContactPhone.Should().Be("+27821234567");
        dto.CompanyName.Should().Be("Buyer Corp");
        dto.Message.Should().Be("Interested in your green solutions.");
        dto.Status.Should().Be("Contacted");
        dto.LeadType.Should().Be("inquiry");
        dto.IpAddress.Should().Be("192.168.1.1");
        dto.CreatedAt.Should().Be(lead.CreatedAt);
        dto.UpdatedAt.Should().Be(lead.UpdatedAt);
    }

    [Fact]
    public void MapToDto_NullOptionalFields_MapsAsNull()
    {
        // Arrange
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            SupplierProfileId = Guid.NewGuid(),
            ContactName = "Anon",
            ContactEmail = "anon@test.com",
            Message = "Anonymous inquiry",
            Status = LeadStatus.New,
            LeadType = "inquiry",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var dto = LeadMapper.MapToDto(lead);

        // Assert
        dto.BuyerOrganizationId.Should().BeNull();
        dto.BuyerUserId.Should().BeNull();
        dto.ContactPhone.Should().BeNull();
        dto.CompanyName.Should().BeNull();
        dto.IpAddress.Should().BeNull();
    }

    [Fact]
    public void MapToDto_StatusEnumToString_ConvertsCorrectly()
    {
        // Arrange
        var statuses = new[] { LeadStatus.New, LeadStatus.Contacted, LeadStatus.Closed };

        foreach (var status in statuses)
        {
            var lead = new Lead
            {
                Id = Guid.NewGuid(),
                SupplierProfileId = Guid.NewGuid(),
                ContactName = "Test",
                ContactEmail = "test@test.com",
                Message = "Test",
                Status = status,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var dto = LeadMapper.MapToDto(lead);

            // Assert
            dto.Status.Should().Be(status.ToString());
        }
    }
}
