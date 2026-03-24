using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(GreenSuppliersDbContext context)
    {
        if (await context.Users.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        // Countries
        var countries = new List<Country>
        {
            new() { Code = "ZA", Name = "South Africa", Slug = "south-africa", Region = "Southern Africa", IsActive = true, SortOrder = 1 },
            new() { Code = "KE", Name = "Kenya", Slug = "kenya", Region = "East Africa", IsActive = false, SortOrder = 2 },
            new() { Code = "NG", Name = "Nigeria", Slug = "nigeria", Region = "West Africa", IsActive = false, SortOrder = 3 },
            new() { Code = "GH", Name = "Ghana", Slug = "ghana", Region = "West Africa", IsActive = false, SortOrder = 4 },
            new() { Code = "EG", Name = "Egypt", Slug = "egypt", Region = "North Africa", IsActive = false, SortOrder = 5 },
            new() { Code = "MA", Name = "Morocco", Slug = "morocco", Region = "North Africa", IsActive = false, SortOrder = 6 },
            new() { Code = "RW", Name = "Rwanda", Slug = "rwanda", Region = "East Africa", IsActive = false, SortOrder = 7 },
            new() { Code = "TZ", Name = "Tanzania", Slug = "tanzania", Region = "East Africa", IsActive = false, SortOrder = 8 },
            new() { Code = "UG", Name = "Uganda", Slug = "uganda", Region = "East Africa", IsActive = false, SortOrder = 9 },
            new() { Code = "BW", Name = "Botswana", Slug = "botswana", Region = "Southern Africa", IsActive = false, SortOrder = 10 },
        };
        context.Countries.AddRange(countries);

        // Industries
        var industries = new List<Industry>
        {
            new() { Id = Guid.NewGuid(), Name = "Renewable Energy", Slug = "renewable-energy", IsActive = true, SortOrder = 1, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Construction", Slug = "construction", IsActive = true, SortOrder = 2, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Agriculture", Slug = "agriculture", IsActive = true, SortOrder = 3, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Waste Management", Slug = "waste-management", IsActive = true, SortOrder = 4, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Water Solutions", Slug = "water-solutions", IsActive = true, SortOrder = 5, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Logistics", Slug = "logistics", IsActive = true, SortOrder = 6, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Manufacturing", Slug = "manufacturing", IsActive = true, SortOrder = 7, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Mining Services", Slug = "mining-services", IsActive = true, SortOrder = 8, CreatedAt = now },
        };
        context.Industries.AddRange(industries);

        // Certification Types
        var certificationTypes = new List<CertificationType>
        {
            new() { Id = Guid.NewGuid(), Name = "ISO 14001", Slug = "iso-14001", IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "ISO 9001", Slug = "iso-9001", IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "B-Corp", Slug = "b-corp", IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "FSC", Slug = "fsc", IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Green Building Council SA", Slug = "green-building-council-sa", IsActive = true, CreatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Carbon Neutral Certification", Slug = "carbon-neutral-certification", IsActive = true, CreatedAt = now },
        };
        context.CertificationTypes.AddRange(certificationTypes);

        // Admin Organization
        var orgId = Guid.NewGuid();
        var adminOrg = new Organization
        {
            Id = orgId,
            Name = "Green Suppliers Admin",
            OrganizationType = OrganizationType.Admin,
            CountryCode = "ZA",
            CreatedAt = now,
            UpdatedAt = now,
        };
        context.Organizations.Add(adminOrg);

        // Admin User
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            Email = "admin@greensuppliers.co.za",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ChangeMe123!"),
            FirstName = "Admin",
            LastName = "User",
            Role = UserRole.Admin,
            IsActive = true,
            EmailVerified = true,
            CreatedAt = now,
            UpdatedAt = now,
        };
        context.Users.Add(adminUser);

        await context.SaveChangesAsync();
    }
}
