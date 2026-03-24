using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class AuditEventConfig : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("AuditEvents");

        builder.HasKey(ae => ae.Id);
        builder.Property(ae => ae.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(ae => ae.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ae => ae.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ae => ae.EntityId)
            .IsRequired();

        builder.Property(ae => ae.IpAddress)
            .HasMaxLength(45);

        builder.Property(ae => ae.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Indexes
        builder.HasIndex(ae => new { ae.EntityType, ae.EntityId })
            .HasDatabaseName("IX_AuditEvents_Entity");

        builder.HasIndex(ae => ae.CreatedAt)
            .HasDatabaseName("IX_AuditEvents_CreatedAt")
            .IsDescending();

        // Relationships
        builder.HasOne(ae => ae.User)
            .WithMany()
            .HasForeignKey(ae => ae.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
