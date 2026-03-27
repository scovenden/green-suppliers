using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GreenSuppliers.Api.Controllers;

[ApiController]
[Route("api/v1/plans")]
public class PlansController : ControllerBase
{
    private readonly BillingService _billingService;

    public PlansController(BillingService billingService)
    {
        _billingService = billingService;
    }

    /// <summary>
    /// Get all active subscription plans. Public endpoint, no auth required.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPlans(CancellationToken ct)
    {
        var plans = await _billingService.GetPlansAsync(ct);
        return Ok(ApiResponse<List<PlanDto>>.Ok(plans));
    }
}
