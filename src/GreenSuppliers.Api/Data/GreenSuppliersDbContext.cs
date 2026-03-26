using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Data;

public class GreenSuppliersDbContext : DbContext
{
    public GreenSuppliersDbContext(DbContextOptions<GreenSuppliersDbContext> options) : base(options) { }

    // Phase 1 entities
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SupplierProfile> SupplierProfiles => Set<SupplierProfile>();
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<ServiceTag> ServiceTags => Set<ServiceTag>();
    public DbSet<CertificationType> CertificationTypes => Set<CertificationType>();
    public DbSet<SupplierCertification> SupplierCertifications => Set<SupplierCertification>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<EmailQueueItem> EmailQueue => Set<EmailQueueItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Phase 2 stubs
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SponsoredPlacement> SponsoredPlacements => Set<SponsoredPlacement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GreenSuppliersDbContext).Assembly);
    }
}
