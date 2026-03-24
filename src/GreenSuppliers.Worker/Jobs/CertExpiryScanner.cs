using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Worker.Jobs;

/// <summary>
/// Daily job (2am UTC) that checks for expiring/expired certifications,
/// queues reminder emails, marks expired certs, and triggers rescoring.
/// </summary>
public class CertExpiryScanner : BackgroundService
{
    private static readonly TimeSpan RunAtHourUtc = TimeSpan.FromHours(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CertExpiryScanner> _logger;

    public CertExpiryScanner(IServiceScopeFactory scopeFactory, ILogger<CertExpiryScanner> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CertExpiryScanner started. Scheduled daily at {Hour}:00 UTC", RunAtHourUtc.Hours);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            _logger.LogInformation("CertExpiryScanner next run in {Delay}", delay);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                await ExecuteJobAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "CertExpiryScanner failed during execution");
            }
        }

        _logger.LogInformation("CertExpiryScanner stopped");
    }

    private TimeSpan CalculateDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.Add(RunAtHourUtc);
        if (nextRun <= now)
        {
            nextRun = nextRun.AddDays(1);
        }
        return nextRun - now;
    }

    private async Task ExecuteJobAsync(CancellationToken ct)
    {
        _logger.LogInformation("CertExpiryScanner executing at {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GreenSuppliersDbContext>();
        var esgService = scope.ServiceProvider.GetRequiredService<EsgScoringService>();
        var verificationService = scope.ServiceProvider.GetRequiredService<VerificationService>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Find all accepted certs with an expiry date
        var certs = await db.SupplierCertifications
            .Include(c => c.SupplierProfile)
                .ThenInclude(sp => sp.Organization)
            .Include(c => c.CertificationType)
            .Where(c => c.Status == CertificationStatus.Accepted && c.ExpiresAt != null)
            .ToListAsync(ct);

        var remindersQueued = 0;
        var expiredCount = 0;
        var rescoredCount = 0;

        foreach (var cert in certs)
        {
            var daysUntilExpiry = cert.ExpiresAt!.Value.DayNumber - today.DayNumber;

            // Already expired
            if (daysUntilExpiry <= 0)
            {
                cert.Status = CertificationStatus.Expired;
                cert.UpdatedAt = DateTime.UtcNow;
                expiredCount++;

                _logger.LogInformation(
                    "Cert {CertId} ({CertType}) for supplier {SupplierId} marked as Expired",
                    cert.Id, cert.CertificationType.Name, cert.SupplierProfileId);

                // Trigger rescore for the affected supplier
                await RescoreSupplierAsync(db, cert.SupplierProfile, esgService, verificationService, ct);
                rescoredCount++;

                // Queue expiry notification
                QueueEmailReminder(db, cert, "expired",
                    $"Your {cert.CertificationType.Name} certification has expired");
                remindersQueued++;

                continue;
            }

            // Queue reminders at 30, 14, and 7 day thresholds
            // Only queue for exact threshold days to avoid duplicate emails on each daily run
            if (daysUntilExpiry == 30 || daysUntilExpiry == 14 || daysUntilExpiry == 7)
            {
                var urgency = daysUntilExpiry <= 7 ? "urgent" : "reminder";
                QueueEmailReminder(db, cert, urgency,
                    $"Your {cert.CertificationType.Name} certification expires in {daysUntilExpiry} days");
                remindersQueued++;

                _logger.LogInformation(
                    "Queued {Urgency} reminder for cert {CertId} ({CertType}), expires in {Days} days",
                    urgency, cert.Id, cert.CertificationType.Name, daysUntilExpiry);
            }
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "CertExpiryScanner complete: {Expired} expired, {Rescored} rescored, {Reminders} reminders queued",
            expiredCount, rescoredCount, remindersQueued);
    }

    private static async Task RescoreSupplierAsync(
        GreenSuppliersDbContext db,
        Api.Models.Entities.SupplierProfile profile,
        EsgScoringService esgService,
        VerificationService verificationService,
        CancellationToken ct)
    {
        // Reload all certs for this supplier to get accurate scoring
        var allCerts = await db.SupplierCertifications
            .Where(c => c.SupplierProfileId == profile.Id)
            .ToListAsync(ct);

        var esgResult = esgService.CalculateScore(profile, allCerts);
        var newVerificationStatus = verificationService.Evaluate(profile, allCerts);

        profile.EsgLevel = esgResult.Level;
        profile.EsgScore = esgResult.NumericScore;
        profile.VerificationStatus = newVerificationStatus;
        profile.LastScoredAt = DateTime.UtcNow;
        profile.UpdatedAt = DateTime.UtcNow;
    }

    private static void QueueEmailReminder(
        GreenSuppliersDbContext db,
        Api.Models.Entities.SupplierCertification cert,
        string templateType,
        string subject)
    {
        var org = cert.SupplierProfile.Organization;
        var toEmail = org.Email ?? string.Empty;
        var toName = org.Name;

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            return;
        }

        db.EmailQueue.Add(new Api.Models.Entities.EmailQueueItem
        {
            Id = Guid.NewGuid(),
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            BodyHtml = $"<p>{subject}.</p><p>Certification: {cert.CertificationType.Name}</p>" +
                       $"<p>Certificate number: {cert.CertificateNumber ?? "N/A"}</p>" +
                       $"<p>Expiry date: {cert.ExpiresAt?.ToString("yyyy-MM-dd") ?? "N/A"}</p>" +
                       "<p>Please renew your certification to maintain your verification status.</p>",
            TemplateType = $"cert_{templateType}",
            Status = "pending",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow
        });
    }
}
