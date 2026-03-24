using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class ContentPageConfig : IEntityTypeConfiguration<ContentPage>
{
    public void Configure(EntityTypeBuilder<ContentPage> builder)
    {
        builder.ToTable("ContentPages");

        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(cp => cp.Slug)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(cp => cp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cp => cp.MetaTitle)
            .HasMaxLength(200);

        builder.Property(cp => cp.MetaDesc)
            .HasMaxLength(300);

        builder.Property(cp => cp.Body)
            .IsRequired();

        builder.Property(cp => cp.PageType)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(cp => cp.IsPublished)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(cp => cp.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(cp => cp.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Unique slug
        builder.HasIndex(cp => cp.Slug)
            .IsUnique();
    }
}
