using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class CountryConfig : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        // PK is Code (char(2)), NOT a GUID
        builder.HasKey(c => c.Code);
        builder.Property(c => c.Code)
            .HasColumnType("char(2)")
            .IsFixedLength();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Region)
            .HasMaxLength(50);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Unique slug
        builder.HasIndex(c => c.Slug)
            .IsUnique();
    }
}
