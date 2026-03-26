using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class SavedSupplierConfig : IEntityTypeConfiguration<SavedSupplier>
{
    public void Configure(EntityTypeBuilder<SavedSupplier> builder)
    {
        builder.ToTable("SavedSuppliers");

        builder.HasKey(ss => ss.Id);
        builder.Property(ss => ss.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(ss => ss.Notes)
            .HasMaxLength(500);

        builder.Property(ss => ss.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Composite unique index: a buyer can only save a supplier once
        builder.HasIndex(ss => new { ss.BuyerUserId, ss.SupplierProfileId })
            .IsUnique()
            .HasDatabaseName("IX_SavedSuppliers_BuyerUser_SupplierProfile");

        // Relationships
        builder.HasOne(ss => ss.BuyerUser)
            .WithMany()
            .HasForeignKey(ss => ss.BuyerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.SupplierProfile)
            .WithMany()
            .HasForeignKey(ss => ss.SupplierProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
