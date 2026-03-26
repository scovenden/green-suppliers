using Resend;

namespace GreenSuppliers.Worker.Services;

/// <summary>
/// Production email sender that uses the Resend API to deliver transactional emails.
/// </summary>
public class ResendEmailSender : IEmailSender
{
    private readonly IResend _resend;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(
        IResend resend,
        IConfiguration configuration,
        ILogger<ResendEmailSender> logger)
    {
        _resend = resend;
        _fromEmail = configuration["Resend:FromEmail"] ?? "noreply@greensuppliers.co.za";
        _fromName = configuration["Resend:FromName"] ?? "Green Suppliers";
        _logger = logger;
    }

    public async Task<bool> SendAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default)
    {
        try
        {
            var message = new EmailMessage
            {
                From = $"{_fromName} <{_fromEmail}>",
                To = { toEmail },
                Subject = subject,
                HtmlBody = bodyHtml
            };

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var emailId = await _resend.EmailSendAsync(message, cts.Token);

            _logger.LogInformation(
                "Email sent via Resend. Id={EmailId}, To={ToEmail}, Subject={Subject}",
                emailId, toEmail, subject);

            return true;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Resend API timed out after 5s. To={ToEmail}, Subject={Subject}",
                toEmail, subject);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email via Resend. To={ToEmail}, Subject={Subject}",
                toEmail, subject);
            return false;
        }
    }
}
