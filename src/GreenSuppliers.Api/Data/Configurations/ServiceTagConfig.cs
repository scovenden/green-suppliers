using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class ServiceTagConfig : IEntityTypeConfiguration<ServiceTag>
{
    public void Configure(EntityTypeBuilder<ServiceTag> builder)
    {
        builder.ToTable("ServiceTags");

        builder.HasKey(st => st.Id);
        builder.Property(st => st.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(st => st.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(st => st.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(st => st.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(st => st.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Unique slug
        builder.HasIndex(st => st.Slug)
            .IsUnique();
    }
}
