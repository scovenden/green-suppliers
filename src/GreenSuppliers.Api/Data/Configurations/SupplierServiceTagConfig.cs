using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SupplierServiceTagConfig : IEntityTypeConfiguration<SupplierServiceTag>
{
    public void Configure(EntityTypeBuilder<SupplierServiceTag> builder)
    {
        builder.ToTable("SupplierServiceTags");

        builder.HasKey(sst => new { sst.SupplierProfileId, sst.ServiceTagId });

        builder.HasOne(sst => sst.SupplierProfile)
            .WithMany(sp => sp.SupplierServiceTags)
            .HasForeignKey(sst => sst.SupplierProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sst => sst.ServiceTag)
            .WithMany(st => st.SupplierServiceTags)
            .HasForeignKey(sst => sst.ServiceTagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
