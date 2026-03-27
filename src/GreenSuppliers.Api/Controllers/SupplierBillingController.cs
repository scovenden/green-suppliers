using GreenSuppliers.Api.Extensions;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/supplier/billing")]
[Authorize(Policy = "Supplier")]
public class SupplierBillingController : ControllerBase
{
    private readonly BillingService _billingService;

    public SupplierBillingController(BillingService billingService)
    {
        _billingService = billingService;
    }

    /// <summary>
    /// Get the current subscription for the authenticated supplier's organization.
    /// </summary>
    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var subscription = await _billingService.GetSubscriptionAsync(orgId, ct);

        if (subscription is null)
            return Ok(ApiResponse<SubscriptionDto?>.Ok(null));

        return Ok(ApiResponse<SubscriptionDto>.Ok(subscription));
    }

    /// <summary>
    /// Create a checkout session for a subscription plan. Returns a PayFast checkout URL.
    /// </summary>
    [HttpPost("checkout")]
    public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var email = User.FindFirst("email")?.Value ?? string.Empty;
        var firstName = User.FindFirst("given_name")?.Value;
        var lastName = User.FindFirst("family_name")?.Value;

        var result = await _billingService.CreateCheckoutAsync(
            orgId, request.PlanId, request.BillingCycle,
            email, firstName, lastName, userId, ct);

        if (result is null)
            return NotFound(ApiResponse<CheckoutResult>.Fail("PLAN_NOT_FOUND", "The selected plan was not found or is inactive."));

        return Ok(ApiResponse<CheckoutResult>.Ok(result));
    }

    /// <summary>
    /// Cancel the current active subscription.
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> CancelSubscription(CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var userId = User.GetUserId();
        var result = await _billingService.CancelSubscriptionAsync(orgId, userId, ct);

        if (!result)
            return NotFound(ApiResponse<object>.Fail("NO_ACTIVE_SUBSCRIPTION", "No active subscription found to cancel."));

        return Ok(ApiResponse<object>.Ok(new { cancelled = true }));
    }

    /// <summary>
    /// Get payment history for the authenticated supplier's organization.
    /// </summary>
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var orgId = User.GetOrganizationId();
        var result = await _billingService.GetPaymentHistoryAsync(orgId, page, pageSize, ct);

        return Ok(ApiResponse<List<PaymentDto>>.Ok(
            result.Items,
            new PaginationMeta(result.Page, result.PageSize, result.Total, result.TotalPages)));
    }

    /// <summary>
    /// Check if the authenticated supplier's organization has access to a specific feature.
    /// </summary>
    [HttpGet("features/{feature}")]
    public async Task<IActionResult> CheckFeatureAccess(string feature, CancellationToken ct)
    {
        var orgId = User.GetOrganizationId();
        var hasAccess = await _billingService.HasFeatureAccessAsync(orgId, feature, ct);

        return Ok(ApiResponse<object>.Ok(new { feature, hasAccess }));
    }
}
