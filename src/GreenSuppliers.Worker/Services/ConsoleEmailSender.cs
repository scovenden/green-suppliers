namespace GreenSuppliers.Worker.Services;

/// <summary>
/// Development stub that logs emails to the console instead of sending them.
/// Replace with SendGrid/Resend implementation for production.
/// </summary>
public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[EMAIL] To={ToEmail} ({ToName}), Subject={Subject}, BodyLength={BodyLength}",
            toEmail, toName, subject, bodyHtml.Length);

        return Task.FromResult(true);
    }
}
