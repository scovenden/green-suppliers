using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SupplierProfileConfig : IEntityTypeConfiguration<SupplierProfile>
{
    public void Configure(EntityTypeBuilder<SupplierProfile> builder)
    {
        builder.ToTable("SupplierProfiles", t =>
        {
            t.HasCheckConstraint("CK_SupplierProfiles_RenewableEnergyPercent",
                "[RenewableEnergyPercent] IS NULL OR ([RenewableEnergyPercent] >= 0 AND [RenewableEnergyPercent] <= 100)");
            t.HasCheckConstraint("CK_SupplierProfiles_WasteRecyclingPercent",
                "[WasteRecyclingPercent] IS NULL OR ([WasteRecyclingPercent] >= 0 AND [WasteRecyclingPercent] <= 100)");
        });

        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(sp => sp.Slug)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(sp => sp.TradingName)
            .HasMaxLength(200);

        builder.Property(sp => sp.Description)
            .HasMaxLength(4000);

        builder.Property(sp => sp.ShortDescription)
            .HasMaxLength(500);

        builder.Property(sp => sp.LogoUrl)
            .HasMaxLength(1000);

        builder.Property(sp => sp.BannerUrl)
            .HasMaxLength(1000);

        builder.Property(sp => sp.EmployeeCount)
            .HasMaxLength(30);

        builder.Property(sp => sp.BbbeeLevel)
            .HasMaxLength(20);

        builder.Property(sp => sp.CountryCode)
            .IsRequired()
            .HasColumnType("char(2)");

        builder.Property(sp => sp.City)
            .HasMaxLength(100);

        builder.Property(sp => sp.Province)
            .HasMaxLength(100);

        builder.Property(sp => sp.CarbonReporting)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sp => sp.WaterManagement)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sp => sp.SustainablePackaging)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sp => sp.VerificationStatus)
            .IsRequired()
            .HasColumnType("nvarchar(20)")
            .HasDefaultValue(VerificationStatus.Unverified)
            .HasConversion<string>();

        builder.Property(sp => sp.EsgLevel)
            .IsRequired()
            .HasColumnType("nvarchar(20)")
            .HasDefaultValue(EsgLevel.None)
            .HasConversion<string>();

        builder.Property(sp => sp.EsgScore)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sp => sp.IsPublished)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sp => sp.FlaggedReason)
            .HasMaxLength(500);

        builder.Property(sp => sp.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(sp => sp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(sp => sp.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Unique indexes
        builder.HasIndex(sp => sp.Slug)
            .IsUnique();

        builder.HasIndex(sp => sp.OrganizationId)
            .IsUnique();

        // Filtered indexes
        builder.HasIndex(sp => sp.CountryCode)
            .HasDatabaseName("IX_SupplierProfiles_CountryCode")
            .HasFilter("[IsDeleted] = 0 AND [IsPublished] = 1");

        builder.HasIndex(sp => sp.EsgLevel)
            .HasDatabaseName("IX_SupplierProfiles_EsgLevel")
            .HasFilter("[IsDeleted] = 0 AND [IsPublished] = 1");

        builder.HasIndex(sp => sp.VerificationStatus)
            .HasDatabaseName("IX_SupplierProfiles_Verification")
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(sp => sp.EsgScore)
            .HasDatabaseName("IX_SupplierProfiles_EsgScore")
            .IsDescending()
            .HasFilter("[IsDeleted] = 0 AND [IsPublished] = 1");

        // Relationships
        builder.HasOne(sp => sp.Organization)
            .WithOne(o => o.SupplierProfile)
            .HasForeignKey<SupplierProfile>(sp => sp.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
