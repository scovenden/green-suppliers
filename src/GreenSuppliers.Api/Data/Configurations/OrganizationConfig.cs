using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class OrganizationConfig : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(o => o.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(o => o.RegistrationNo)
            .HasMaxLength(50);

        builder.Property(o => o.CountryCode)
            .IsRequired()
            .HasColumnType("char(2)");

        builder.Property(o => o.City)
            .HasMaxLength(100);

        builder.Property(o => o.Province)
            .HasMaxLength(100);

        builder.Property(o => o.Website)
            .HasMaxLength(500);

        builder.Property(o => o.Phone)
            .HasMaxLength(30);

        builder.Property(o => o.Email)
            .HasMaxLength(254);

        builder.Property(o => o.OrganizationType)
            .IsRequired()
            .HasColumnType("nvarchar(20)")
            .HasConversion<string>();

        builder.Property(o => o.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(o => o.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(o => o.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Indexes
        builder.HasIndex(o => o.CountryCode)
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(o => o.OrganizationType)
            .HasFilter("[IsDeleted] = 0");
    }
}
