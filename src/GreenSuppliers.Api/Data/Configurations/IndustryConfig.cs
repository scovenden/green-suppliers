using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class IndustryConfig : IEntityTypeConfiguration<Industry>
{
    public void Configure(EntityTypeBuilder<Industry> builder)
    {
        builder.ToTable("Industries");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Slug)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(i => i.Description)
            .HasMaxLength(500);

        builder.Property(i => i.SortOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(i => i.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Unique slug
        builder.HasIndex(i => i.Slug)
            .IsUnique();

        // Self-referencing parent relationship
        builder.HasOne(i => i.Parent)
            .WithMany(i => i.Children)
            .HasForeignKey(i => i.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
