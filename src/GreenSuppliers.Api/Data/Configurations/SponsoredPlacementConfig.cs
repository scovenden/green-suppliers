using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SponsoredPlacementConfig : IEntityTypeConfiguration<SponsoredPlacement>
{
    public void Configure(EntityTypeBuilder<SponsoredPlacement> builder)
    {
        builder.ToTable("SponsoredPlacements");

        builder.HasKey(sp => sp.Id);
        builder.Property(sp => sp.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(sp => sp.PlacementType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(sp => sp.StartsAt)
            .IsRequired();

        builder.Property(sp => sp.EndsAt)
            .IsRequired();

        builder.Property(sp => sp.ImpressionsCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sp => sp.ClicksCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(sp => sp.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(sp => sp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Relationships
        builder.HasOne(sp => sp.SupplierProfile)
            .WithMany()
            .HasForeignKey(sp => sp.SupplierProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
