using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GreenSuppliers.Api.Data.Configurations;

public class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(254);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasColumnType("nvarchar(30)")
            .HasConversion<string>();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.EmailVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.EmailVerificationToken)
            .HasMaxLength(128);

        builder.Property(u => u.PasswordResetToken)
            .HasMaxLength(128);

        builder.Property(u => u.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(u => u.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        // Unique email
        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Filtered index for token lookups (used by VerifyEmailAsync)
        builder.HasIndex(u => u.EmailVerificationToken)
            .HasFilter("[EmailVerificationToken] IS NOT NULL");

        // Filtered index for token lookups (used by ResetPasswordAsync)
        builder.HasIndex(u => u.PasswordResetToken)
            .HasFilter("[PasswordResetToken] IS NOT NULL");

        // Filtered index
        builder.HasIndex(u => u.OrganizationId)
            .HasFilter("[IsDeleted] = 0");

        // Relationships
        builder.HasOne(u => u.Organization)
            .WithMany(o => o.Users)
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
