using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/webhooks/payfast")]
public class PayFastWebhookController : ControllerBase
{
    private readonly BillingService _billingService;
    private readonly ILogger<PayFastWebhookController> _logger;

    public PayFastWebhookController(BillingService billingService, ILogger<PayFastWebhookController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }

    /// <summary>
    /// PayFast ITN (Instant Transaction Notification) webhook endpoint.
    /// Receives form-encoded POST data from PayFast servers.
    /// This endpoint must be anonymous (no auth) as PayFast cannot authenticate.
    /// </summary>
    [HttpPost("itn")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> HandleItn(CancellationToken ct)
    {
        // Read form data from the request
        var formData = new Dictionary<string, string>();

        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(ct);
            foreach (var key in form.Keys)
            {
                formData[key] = form[key].ToString();
            }
        }

        if (formData.Count == 0)
        {
            _logger.LogWarning("PayFast ITN: received empty form data");
            return BadRequest(ApiResponse<object>.Fail("INVALID_PAYLOAD", "Empty or invalid form data."));
        }

        var sourceIp = HttpContext.Connection.RemoteIpAddress?.ToString();

        _logger.LogInformation("PayFast ITN received from {SourceIp} with m_payment_id={PaymentId}",
            sourceIp, formData.GetValueOrDefault("m_payment_id", "unknown"));

        var success = await _billingService.ProcessItnAsync(formData, sourceIp, ct);

        if (!success)
        {
            _logger.LogWarning("PayFast ITN processing failed");
            return BadRequest(ApiResponse<object>.Fail("ITN_FAILED", "ITN processing failed."));
        }

        // PayFast expects a 200 OK response
        return Ok();
    }
}
