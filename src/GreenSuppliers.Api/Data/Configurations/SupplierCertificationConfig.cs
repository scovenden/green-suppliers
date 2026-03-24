using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SupplierCertificationConfig : IEntityTypeConfiguration<SupplierCertification>
{
    public void Configure(EntityTypeBuilder<SupplierCertification> builder)
    {
        builder.ToTable("SupplierCertifications");

        builder.HasKey(sc => sc.Id);
        builder.Property(sc => sc.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(sc => sc.CertificateNumber)
            .HasMaxLength(100);

        builder.Property(sc => sc.Status)
            .IsRequired()
            .HasColumnType("nvarchar(20)")
            .HasDefaultValue(CertificationStatus.Pending)
            .HasConversion<string>();

        builder.Property(sc => sc.Notes)
            .HasMaxLength(500);

        builder.Property(sc => sc.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(sc => sc.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Indexes
        builder.HasIndex(sc => sc.SupplierProfileId)
            .HasDatabaseName("IX_SC_SupplierProfile");

        builder.HasIndex(sc => sc.ExpiresAt)
            .HasDatabaseName("IX_SC_ExpiresAt")
            .HasFilter("[Status] = 'Accepted'");

        // Relationships
        builder.HasOne(sc => sc.SupplierProfile)
            .WithMany(sp => sp.Certifications)
            .HasForeignKey(sc => sc.SupplierProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.CertificationType)
            .WithMany(ct => ct.SupplierCertifications)
            .HasForeignKey(sc => sc.CertificationTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sc => sc.Document)
            .WithMany()
            .HasForeignKey(sc => sc.DocumentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sc => sc.VerifiedByUser)
            .WithMany()
            .HasForeignKey(sc => sc.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
