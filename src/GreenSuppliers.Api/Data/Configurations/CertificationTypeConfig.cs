using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class CertificationTypeConfig : IEntityTypeConfiguration<CertificationType>
{
    public void Configure(EntityTypeBuilder<CertificationType> builder)
    {
        builder.ToTable("CertificationTypes");

        builder.HasKey(ct => ct.Id);
        builder.Property(ct => ct.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(ct => ct.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ct => ct.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(ct => ct.Description)
            .HasMaxLength(500);

        builder.Property(ct => ct.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ct => ct.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Unique slug
        builder.HasIndex(ct => ct.Slug)
            .IsUnique();
    }
}
