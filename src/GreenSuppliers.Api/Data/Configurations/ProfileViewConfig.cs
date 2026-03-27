using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class ProfileViewConfig : IEntityTypeConfiguration<ProfileView>
{
    public void Configure(EntityTypeBuilder<ProfileView> builder)
    {
        builder.ToTable("ProfileViews");

        builder.HasKey(pv => pv.Id);
        builder.Property(pv => pv.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(pv => pv.ViewerIp)
            .HasMaxLength(45);

        builder.Property(pv => pv.Referrer)
            .HasMaxLength(2000);

        builder.Property(pv => pv.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Index for analytics queries (supplier + date range)
        builder.HasIndex(pv => new { pv.SupplierProfileId, pv.CreatedAt });

        // Relationships
        builder.HasOne(pv => pv.SupplierProfile)
            .WithMany(sp => sp.ProfileViews)
            .HasForeignKey(pv => pv.SupplierProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
