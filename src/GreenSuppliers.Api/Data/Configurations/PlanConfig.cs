using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class PlanConfig : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Plans");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.PriceMonthly)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.PriceYearly)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.Currency)
            .IsRequired()
            .HasColumnType("char(3)")
            .HasDefaultValue("ZAR");

        builder.Property(p => p.FeaturedListing)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.AnalyticsAccess)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.PrioritySupport)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.TrialDays)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
    }
}
