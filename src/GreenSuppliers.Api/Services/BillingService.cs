using System.Globalization;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.DTOs;
using GreenSuppliers.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenSuppliers.Api.Services;

public class BillingService
{
    private readonly GreenSuppliersDbContext _context;
    private readonly PayFastService _payFast;
    private readonly AuditService _audit;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        GreenSuppliersDbContext context,
        PayFastService payFast,
        AuditService audit,
        ILogger<BillingService> logger)
    {
        _context = context;
        _payFast = payFast;
        _audit = audit;
        _logger = logger;
    }

    /// <summary>
    /// Returns all active plans, ordered by SortOrder.
    /// </summary>
    public async Task<List<PlanDto>> GetPlansAsync(CancellationToken ct)
    {
        return await _context.Plans
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => MapPlanToDto(p))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Gets the active subscription for an organization (most recent non-expired).
    /// </summary>
    public async Task<SubscriptionDto?> GetSubscriptionAsync(Guid organizationId, CancellationToken ct)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.OrganizationId == organizationId
                && s.Status != "cancelled"
                && s.Status != "expired")
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return subscription is null ? null : MapSubscriptionToDto(subscription);
    }

    /// <summary>
    /// Creates a pending subscription and payment, then generates a PayFast checkout URL.
    /// If the plan is Free, activates immediately without payment.
    /// </summary>
    public async Task<CheckoutResult?> CreateCheckoutAsync(
        Guid organizationId,
        Guid planId,
        string billingCycle,
        string buyerEmail,
        string? buyerFirstName,
        string? buyerLastName,
        Guid userId,
        CancellationToken ct)
    {
        var plan = await _context.Plans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planId && p.IsActive, ct);

        if (plan is null)
            return null;

        if (billingCycle != "monthly" && billingCycle != "yearly")
            billingCycle = "monthly";

        var amount = billingCycle == "yearly" ? plan.PriceYearly : plan.PriceMonthly;
        var now = DateTime.UtcNow;

        // Cancel any existing active/pending subscription for this org
        var existingSubscriptions = await _context.Subscriptions
            .Where(s => s.OrganizationId == organizationId
                && (s.Status == "active" || s.Status == "pending" || s.Status == "trial"))
            .ToListAsync(ct);

        foreach (var existing in existingSubscriptions)
        {
            existing.Status = "cancelled";
            existing.CancelledAt = now;
            existing.UpdatedAt = now;
        }

        var periodEnd = billingCycle == "yearly"
            ? now.AddYears(1)
            : now.AddMonths(1);

        // Determine if a trial applies
        DateTime? trialEnd = null;
        var initialStatus = "pending";

        if (plan.TrialDays > 0 && amount > 0)
        {
            // Check if org has ever had a subscription to this plan (no duplicate trials)
            var hadPreviousSubscription = await _context.Subscriptions
                .AnyAsync(s => s.OrganizationId == organizationId && s.PlanId == planId
                    && s.Status != "pending", ct);

            if (!hadPreviousSubscription)
            {
                trialEnd = now.AddDays(plan.TrialDays);
                initialStatus = "trial";
            }
        }

        // Free plan: activate immediately
        if (amount == 0)
        {
            initialStatus = "active";
        }

        var subscription = new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            PlanId = planId,
            Status = initialStatus,
            BillingCycle = billingCycle,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = periodEnd,
            TrialEnd = trialEnd,
            CreatedAt = now,
            UpdatedAt = now
        };
        _context.Subscriptions.Add(subscription);

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            SubscriptionId = subscription.Id,
            Amount = amount,
            Currency = plan.Currency,
            Status = amount == 0 ? "completed" : "pending",
            Provider = "payfast",
            PaidAt = amount == 0 ? now : null,
            CreatedAt = now
        };
        _context.Payments.Add(payment);

        await _context.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "SubscriptionCreated", "Subscription", subscription.Id, ct: ct);

        // Free plan: no checkout URL needed
        if (amount == 0)
        {
            return new CheckoutResult
            {
                SubscriptionId = subscription.Id,
                PaymentId = payment.Id,
                CheckoutUrl = string.Empty
            };
        }

        var itemName = $"Green Suppliers {plan.DisplayName} ({billingCycle})";
        var checkoutUrl = _payFast.GenerateCheckoutUrl(
            payment.Id,
            amount,
            itemName,
            buyerEmail,
            buyerFirstName,
            buyerLastName);

        return new CheckoutResult
        {
            SubscriptionId = subscription.Id,
            PaymentId = payment.Id,
            CheckoutUrl = checkoutUrl
        };
    }

    /// <summary>
    /// Processes an ITN (Instant Transaction Notification) callback from PayFast.
    /// Validates the signature, finds the payment, and activates the subscription.
    /// </summary>
    public async Task<bool> ProcessItnAsync(Dictionary<string, string> formData, string? sourceIp, CancellationToken ct)
    {
        // Step 1: Validate source IP
        if (!_payFast.ValidateSourceIp(sourceIp))
        {
            _logger.LogWarning("PayFast ITN rejected: invalid source IP {SourceIp}", sourceIp);
            return false;
        }

        // Step 2: Validate signature
        if (!_payFast.ValidateItnSignature(formData))
        {
            _logger.LogWarning("PayFast ITN rejected: invalid signature");
            return false;
        }

        // Step 3: Find the payment by m_payment_id
        if (!formData.TryGetValue("m_payment_id", out var paymentIdStr)
            || !Guid.TryParse(paymentIdStr, out var paymentId))
        {
            _logger.LogWarning("PayFast ITN: missing or invalid m_payment_id");
            return false;
        }

        var payment = await _context.Payments
            .Include(p => p.Subscription)
            .FirstOrDefaultAsync(p => p.Id == paymentId, ct);

        if (payment is null)
        {
            _logger.LogWarning("PayFast ITN: payment {PaymentId} not found", paymentId);
            return false;
        }

        // Step 4: Verify amount matches
        if (formData.TryGetValue("amount_gross", out var amountStr)
            && decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var itnAmount))
        {
            if (itnAmount != payment.Amount)
            {
                _logger.LogWarning("PayFast ITN: amount mismatch. Expected={Expected}, Received={Received}",
                    payment.Amount, itnAmount);
                return false;
            }
        }

        var now = DateTime.UtcNow;

        // Step 5: Process based on payment_status
        var paymentStatus = formData.GetValueOrDefault("payment_status", "");

        if (paymentStatus == "COMPLETE")
        {
            payment.Status = "completed";
            payment.PaidAt = now;
            payment.ExternalId = formData.GetValueOrDefault("pf_payment_id");

            var subscription = payment.Subscription;
            subscription.Status = "active";
            subscription.ExternalId = formData.GetValueOrDefault("pf_payment_id");
            subscription.PayFastToken = formData.GetValueOrDefault("token");
            subscription.UpdatedAt = now;

            _logger.LogInformation("PayFast ITN: payment {PaymentId} completed, subscription {SubscriptionId} activated",
                paymentId, subscription.Id);
        }
        else if (paymentStatus == "CANCELLED")
        {
            payment.Status = "failed";
            payment.Subscription.Status = "cancelled";
            payment.Subscription.CancelledAt = now;
            payment.Subscription.UpdatedAt = now;

            _logger.LogInformation("PayFast ITN: payment {PaymentId} cancelled", paymentId);
        }
        else
        {
            _logger.LogWarning("PayFast ITN: unhandled payment_status={Status} for payment {PaymentId}",
                paymentStatus, paymentId);
            return false;
        }

        await _context.SaveChangesAsync(ct);
        await _audit.LogAsync(null, "PayFastItnProcessed", "Payment", paymentId,
            newValues: $"status={paymentStatus}", ct: ct);

        return true;
    }

    /// <summary>
    /// Cancels a subscription. Sets status to cancelled and records cancellation date.
    /// The subscription remains active until the end of the current billing period.
    /// </summary>
    public async Task<bool> CancelSubscriptionAsync(Guid organizationId, Guid userId, CancellationToken ct)
    {
        var subscription = await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.OrganizationId == organizationId
                && (s.Status == "active" || s.Status == "trial"))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (subscription is null)
            return false;

        var now = DateTime.UtcNow;
        subscription.Status = "cancelled";
        subscription.CancelledAt = now;
        subscription.UpdatedAt = now;

        await _context.SaveChangesAsync(ct);
        await _audit.LogAsync(userId, "SubscriptionCancelled", "Subscription", subscription.Id, ct: ct);

        _logger.LogInformation("Subscription {SubscriptionId} cancelled by user {UserId}",
            subscription.Id, userId);

        return true;
    }

    /// <summary>
    /// Gets payment history for an organization, ordered by most recent first.
    /// </summary>
    public async Task<PagedResult<PaymentDto>> GetPaymentHistoryAsync(
        Guid organizationId, int page, int pageSize, CancellationToken ct)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(p => p.Subscription.OrganizationId == organizationId);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentDto
            {
                Id = p.Id,
                SubscriptionId = p.SubscriptionId,
                Amount = p.Amount,
                Currency = p.Currency,
                Status = p.Status,
                Provider = p.Provider,
                ExternalId = p.ExternalId,
                PaidAt = p.PaidAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(ct);

        return new PagedResult<PaymentDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    /// <summary>
    /// Checks if an organization has access to a specific feature based on their current plan.
    /// Free plan gets limited access; Pro gets full; Premium gets everything.
    /// </summary>
    public async Task<bool> HasFeatureAccessAsync(
        Guid organizationId, string feature, CancellationToken ct)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.OrganizationId == organizationId
                && (s.Status == "active" || s.Status == "trial"))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        // No active subscription = free tier (limited access)
        if (subscription is null)
            return IsFreeTierFeature(feature);

        var plan = subscription.Plan;

        return feature.ToLowerInvariant() switch
        {
            "featured_listing" => plan.FeaturedListing,
            "analytics" => plan.AnalyticsAccess,
            "priority_support" => plan.PrioritySupport,
            "unlimited_leads" => plan.MaxLeadsPerMonth is null,
            "unlimited_documents" => plan.MaxDocuments is null,
            _ => true // Unknown features default to allowed for paid plans
        };
    }

    /// <summary>
    /// Gets the current plan name for an organization. Returns "free" if no active subscription.
    /// </summary>
    public async Task<string> GetCurrentPlanNameAsync(Guid organizationId, CancellationToken ct)
    {
        var subscription = await _context.Subscriptions
            .AsNoTracking()
            .Include(s => s.Plan)
            .Where(s => s.OrganizationId == organizationId
                && (s.Status == "active" || s.Status == "trial"))
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return subscription?.Plan.Name ?? "free";
    }

    private static bool IsFreeTierFeature(string feature)
    {
        // Free tier has access to basic features only
        return feature.ToLowerInvariant() switch
        {
            "featured_listing" => false,
            "analytics" => false,
            "priority_support" => false,
            "unlimited_leads" => false,
            "unlimited_documents" => false,
            _ => true
        };
    }

    private static PlanDto MapPlanToDto(Plan plan)
    {
        return new PlanDto
        {
            Id = plan.Id,
            Name = plan.Name,
            DisplayName = plan.DisplayName,
            PriceMonthly = plan.PriceMonthly,
            PriceYearly = plan.PriceYearly,
            Currency = plan.Currency,
            MaxLeadsPerMonth = plan.MaxLeadsPerMonth,
            MaxDocuments = plan.MaxDocuments,
            FeaturedListing = plan.FeaturedListing,
            AnalyticsAccess = plan.AnalyticsAccess,
            PrioritySupport = plan.PrioritySupport,
            TrialDays = plan.TrialDays,
            SortOrder = plan.SortOrder
        };
    }

    private static SubscriptionDto MapSubscriptionToDto(Subscription subscription)
    {
        return new SubscriptionDto
        {
            Id = subscription.Id,
            OrganizationId = subscription.OrganizationId,
            PlanId = subscription.PlanId,
            PlanName = subscription.Plan.Name,
            PlanDisplayName = subscription.Plan.DisplayName,
            Status = subscription.Status,
            BillingCycle = subscription.BillingCycle,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            TrialEnd = subscription.TrialEnd,
            CancelledAt = subscription.CancelledAt,
            CreatedAt = subscription.CreatedAt
        };
    }
}
