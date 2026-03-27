using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SupplierSdgConfig : IEntityTypeConfiguration<SupplierSdg>
{
    public void Configure(EntityTypeBuilder<SupplierSdg> builder)
    {
        builder.ToTable("SupplierSdgs");

        builder.HasKey(ss => new { ss.SupplierProfileId, ss.SdgId });

        builder.HasOne(ss => ss.SupplierProfile)
            .WithMany(sp => sp.SupplierSdgs)
            .HasForeignKey(ss => ss.SupplierProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.Sdg)
            .WithMany(s => s.SupplierSdgs)
            .HasForeignKey(ss => ss.SdgId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
