using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class LeadConfig : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(l => l.ContactName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.ContactEmail)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(l => l.ContactPhone)
            .HasMaxLength(30);

        builder.Property(l => l.CompanyName)
            .HasMaxLength(200);

        builder.Property(l => l.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(l => l.Status)
            .IsRequired()
            .HasColumnType("nvarchar(20)")
            .HasDefaultValue(LeadStatus.New)
            .HasConversion<string>();

        builder.Property(l => l.LeadType)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("inquiry");

        builder.Property(l => l.IpAddress)
            .HasMaxLength(45);

        builder.Property(l => l.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(l => l.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Indexes
        builder.HasIndex(l => l.SupplierProfileId)
            .HasDatabaseName("IX_Leads_SupplierProfile");

        builder.HasIndex(l => l.Status)
            .HasDatabaseName("IX_Leads_Status")
            .HasFilter("[Status] = 'New'");

        // Relationships
        builder.HasOne(l => l.SupplierProfile)
            .WithMany(sp => sp.Leads)
            .HasForeignKey(l => l.SupplierProfileId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.BuyerOrganization)
            .WithMany()
            .HasForeignKey(l => l.BuyerOrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.BuyerUser)
            .WithMany()
            .HasForeignKey(l => l.BuyerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
