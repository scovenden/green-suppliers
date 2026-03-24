using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SupplierIndustryConfig : IEntityTypeConfiguration<SupplierIndustry>
{
    public void Configure(EntityTypeBuilder<SupplierIndustry> builder)
    {
        builder.ToTable("SupplierIndustries");

        builder.HasKey(si => new { si.SupplierProfileId, si.IndustryId });

        builder.HasOne(si => si.SupplierProfile)
            .WithMany(sp => sp.SupplierIndustries)
            .HasForeignKey(si => si.SupplierProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(si => si.Industry)
            .WithMany(i => i.SupplierIndustries)
            .HasForeignKey(si => si.IndustryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
