using GreenSuppliers.Api.Data;
using GreenSuppliers.Worker.Services;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Worker.Jobs;

/// <summary>
/// Continuous polling job (every 30 seconds) that processes the email queue.
/// Sends pending emails via IEmailSender and tracks delivery status.
/// </summary>
public class EmailDispatch : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);
    private const int MaxRetries = 3;
    private const int BatchSize = 50;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailDispatch> _logger;

    public EmailDispatch(IServiceScopeFactory scopeFactory, ILogger<EmailDispatch> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailDispatch started. Polling every {Interval}s", PollInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "EmailDispatch failed during queue processing");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("EmailDispatch stopped");
    }

    private async Task ProcessQueueAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GreenSuppliersDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var pendingEmails = await db.EmailQueue
            .Where(e => e.Status == "pending")
            .OrderBy(e => e.CreatedAt)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (pendingEmails.Count == 0)
        {
            return;
        }

        _logger.LogInformation("EmailDispatch processing {Count} pending emails", pendingEmails.Count);

        var sent = 0;
        var failed = 0;

        foreach (var email in pendingEmails)
        {
            try
            {
                var success = await emailSender.SendAsync(
                    email.ToEmail,
                    email.ToName,
                    email.Subject,
                    email.BodyHtml,
                    ct);

                if (success)
                {
                    email.Status = "sent";
                    email.SentAt = DateTime.UtcNow;
                    email.ErrorMessage = null;
                    sent++;
                }
                else
                {
                    HandleFailure(email, "Email sender returned false");
                    failed++;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                HandleFailure(email, ex.Message);
                failed++;

                _logger.LogWarning(ex, "Failed to send email {EmailId} to {ToEmail}", email.Id, email.ToEmail);
            }
        }

        await db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "EmailDispatch batch complete: {Sent} sent, {Failed} failed",
            sent, failed);
    }

    private static void HandleFailure(Api.Models.Entities.EmailQueueItem email, string errorMessage)
    {
        email.RetryCount++;
        email.ErrorMessage = errorMessage.Length > 2000
            ? errorMessage[..2000]
            : errorMessage;

        if (email.RetryCount >= MaxRetries)
        {
            email.Status = "failed";
        }
    }
}
