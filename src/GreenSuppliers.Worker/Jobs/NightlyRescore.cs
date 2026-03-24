using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Worker.Jobs;

/// <summary>
/// Daily job (3am UTC) that rescores all published, non-deleted supplier profiles.
/// Recalculates ESG level and verification status based on current certifications.
/// </summary>
public class NightlyRescore : BackgroundService
{
    private static readonly TimeSpan RunAtHourUtc = TimeSpan.FromHours(3);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NightlyRescore> _logger;

    public NightlyRescore(IServiceScopeFactory scopeFactory, ILogger<NightlyRescore> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NightlyRescore started. Scheduled daily at {Hour}:00 UTC", RunAtHourUtc.Hours);

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            _logger.LogInformation("NightlyRescore next run in {Delay}", delay);

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
                _logger.LogError(ex, "NightlyRescore failed during execution");
            }
        }

        _logger.LogInformation("NightlyRescore stopped");
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
        _logger.LogInformation("NightlyRescore executing at {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GreenSuppliersDbContext>();
        var esgService = scope.ServiceProvider.GetRequiredService<EsgScoringService>();
        var verificationService = scope.ServiceProvider.GetRequiredService<VerificationService>();

        // Load all published, non-deleted profiles
        var profiles = await db.SupplierProfiles
            .Where(p => p.IsPublished && !p.IsDeleted)
            .ToListAsync(ct);

        var totalRescored = 0;
        var totalChanged = 0;

        foreach (var profile in profiles)
        {
            // Load certifications for this supplier
            var certs = await db.SupplierCertifications
                .Where(c => c.SupplierProfileId == profile.Id)
                .ToListAsync(ct);

            var previousLevel = profile.EsgLevel;
            var previousScore = profile.EsgScore;
            var previousStatus = profile.VerificationStatus;

            // Recalculate
            var esgResult = esgService.CalculateScore(profile, certs);
            var newVerificationStatus = verificationService.Evaluate(profile, certs);

            profile.EsgLevel = esgResult.Level;
            profile.EsgScore = esgResult.NumericScore;
            profile.VerificationStatus = newVerificationStatus;
            profile.LastScoredAt = DateTime.UtcNow;

            totalRescored++;

            if (previousLevel != profile.EsgLevel ||
                previousScore != profile.EsgScore ||
                previousStatus != profile.VerificationStatus)
            {
                profile.UpdatedAt = DateTime.UtcNow;
                totalChanged++;

                _logger.LogInformation(
                    "Supplier {SupplierId} rescored: ESG {OldLevel}->{NewLevel}, Score {OldScore}->{NewScore}, Status {OldStatus}->{NewStatus}",
                    profile.Id, previousLevel, profile.EsgLevel,
                    previousScore, profile.EsgScore,
                    previousStatus, profile.VerificationStatus);
            }
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "NightlyRescore complete: {Total} suppliers rescored, {Changed} changes made",
            totalRescored, totalChanged);
    }
}
