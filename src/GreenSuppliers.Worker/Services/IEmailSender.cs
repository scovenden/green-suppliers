namespace GreenSuppliers.Worker.Services;

public interface IEmailSender
{
    Task<bool> SendAsync(string toEmail, string toName, string subject, string bodyHtml, CancellationToken ct = default);
}
