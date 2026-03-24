using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class EmailQueueItemConfig : IEntityTypeConfiguration<EmailQueueItem>
{
    public void Configure(EntityTypeBuilder<EmailQueueItem> builder)
    {
        builder.ToTable("EmailQueue");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(e => e.ToEmail)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(e => e.ToName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.BodyHtml)
            .IsRequired();

        builder.Property(e => e.TemplateType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("pending");

        builder.Property(e => e.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Index for processing pending emails
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_EmailQueue_Status")
            .HasFilter("[Status] = 'pending'");
    }
}
