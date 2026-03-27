using FluentAssertions;
using GreenSuppliers.Api.Data;
using GreenSuppliers.Api.Models.Entities;
using GreenSuppliers.Api.Models.Enums;
using GreenSuppliers.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GreenSuppliers.Tests.Services;

public class BillingServiceTests
{
    private static GreenSuppliersDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<GreenSuppliersDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new GreenSuppliersDbContext(options);
    }

    private static PayFastService CreatePayFastService(PayFastSettings? settings = null)
    {
        settings ??= new PayFastSettings
        {
            MerchantId = "10000100",
            MerchantKey = "46f0cd694581a",
            Passphrase = "",
            BaseUrl = "https://sandbox.payfast.co.za",
            ReturnUrl = "http://localhost:3000/billing/success",
            CancelUrl = "http://localhost:3000/billing/cancel",
            NotifyUrl = "https://localhost:5001/api/v1/webhooks/payfast/itn",
            UseSandbox = true
        };
        var opts = Options.Create(settings);
        var logger = new Mock<ILogger<PayFastService>>();
        return new PayFastService(opts, logger.Object);
    }

    private static BillingService CreateBillingService(GreenSuppliersDbContext context, PayFastService? payFast = null)
    {
        var audit = new AuditService(context);
        payFast ??= CreatePayFastService();
        var logger = new Mock<ILogger<BillingService>>();
        return new BillingService(context, payFast, audit, logger.Object);
    }

    private static async Task<(Guid OrgId, Guid UserId)> SeedSupplierOrgAsync(GreenSuppliersDbContext context)
    {
        var now = DateTime.UtcNow;
        var org = new Organization
        {
            Id = Guid.NewGuid(),
            Name = "Test Supplier Org",
            CountryCode = "ZA",
            OrganizationType = OrganizationType.Supplier,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Organizations.Add(org);

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = org.Id,
            Email = "supplier@test.com",
            PasswordHash = "hash",
            FirstName = "Test",
            LastName = "Supplier",
            Role = UserRole.SupplierAdmin,
            EmailVerified = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (org.Id, user.Id);
    }

    private static async Task<(Plan Free, Plan Pro, Plan Premium)> SeedPlansAsync(GreenSuppliersDbContext context)
    {
        var now = DateTime.UtcNow;
        var free = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "free",
            DisplayName = "Free",
            PriceMonthly = 0m,
            PriceYearly = 0m,
            Currency = "ZAR",
            MaxLeadsPerMonth = 5,
            MaxDocuments = 3,
            FeaturedListing = false,
            AnalyticsAccess = false,
            PrioritySupport = false,
            TrialDays = 0,
            SortOrder = 1,
            IsActive = true,
            CreatedAt = now
        };

        var pro = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "pro",
            DisplayName = "Pro",
            PriceMonthly = 499m,
            PriceYearly = 4999m,
            Currency = "ZAR",
            MaxLeadsPerMonth = null,
            MaxDocuments = 20,
            FeaturedListing = true,
            AnalyticsAccess = true,
            PrioritySupport = false,
            TrialDays = 14,
            SortOrder = 2,
            IsActive = true,
            CreatedAt = now
        };

        var premium = new Plan
        {
            Id = Guid.NewGuid(),
            Name = "premium",
            DisplayName = "Premium",
            PriceMonthly = 999m,
            PriceYearly = 9999m,
            Currency = "ZAR",
            MaxLeadsPerMonth = null,
            MaxDocuments = null,
            FeaturedListing = true,
            AnalyticsAccess = true,
            PrioritySupport = true,
            TrialDays = 14,
            SortOrder = 3,
            IsActive = true,
            CreatedAt = now
        };

        context.Plans.AddRange(free, pro, premium);
        await context.SaveChangesAsync();
        return (free, pro, premium);
    }

    // =========================================================================
    // GetPlansAsync
    // =========================================================================

    [Fact]
    public async Task GetPlans_ReturnsAllActivePlans_OrderedBySortOrder()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        await SeedPlansAsync(context);

        // Also seed an inactive plan to make sure it's excluded
        context.Plans.Add(new Plan
        {
            Id = Guid.NewGuid(),
            Name = "legacy",
            DisplayName = "Legacy",
            PriceMonthly = 299m,
            PriceYearly = 2999m,
            Currency = "ZAR",
            SortOrder = 0,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        // Act
        var plans = await service.GetPlansAsync(CancellationToken.None);

        // Assert
        plans.Should().HaveCount(3);
        plans[0].Name.Should().Be("free");
        plans[1].Name.Should().Be("pro");
        plans[2].Name.Should().Be("premium");
    }

    [Fact]
    public async Task GetPlans_MapsPlanFieldsCorrectly()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        // Act
        var plans = await service.GetPlansAsync(CancellationToken.None);
        var proPlan = plans.First(p => p.Name == "pro");

        // Assert
        proPlan.DisplayName.Should().Be("Pro");
        proPlan.PriceMonthly.Should().Be(499m);
        proPlan.PriceYearly.Should().Be(4999m);
        proPlan.Currency.Should().Be("ZAR");
        proPlan.FeaturedListing.Should().BeTrue();
        proPlan.AnalyticsAccess.Should().BeTrue();
        proPlan.PrioritySupport.Should().BeFalse();
        proPlan.TrialDays.Should().Be(14);
        proPlan.MaxLeadsPerMonth.Should().BeNull();
        proPlan.MaxDocuments.Should().Be(20);
    }

    // =========================================================================
    // CreateCheckoutAsync — Free plan
    // =========================================================================

    [Fact]
    public async Task CreateCheckout_FreePlan_ActivatesImmediately_NoCheckoutUrl()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (free, _, _) = await SeedPlansAsync(context);

        // Act
        var result = await service.CreateCheckoutAsync(
            orgId, free.Id, "monthly", "test@test.com", "Test", "User", userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CheckoutUrl.Should().BeEmpty();

        var subscription = await context.Subscriptions.FirstAsync();
        subscription.Status.Should().Be("active");
        subscription.PlanId.Should().Be(free.Id);

        var payment = await context.Payments.FirstAsync();
        payment.Status.Should().Be("completed");
        payment.Amount.Should().Be(0m);
        payment.PaidAt.Should().NotBeNull();
    }

    // =========================================================================
    // CreateCheckoutAsync — Paid plan
    // =========================================================================

    [Fact]
    public async Task CreateCheckout_PaidPlan_CreatesSubscriptionAndPayment_ReturnsCheckoutUrl()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        // Act
        var result = await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", "Test", "User", userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CheckoutUrl.Should().Contain("sandbox.payfast.co.za");
        result.CheckoutUrl.Should().Contain("amount=499.00");
        result.CheckoutUrl.Should().Contain("merchant_id=10000100");

        var subscription = await context.Subscriptions.FirstAsync();
        subscription.Status.Should().Be("trial"); // Pro has 14-day trial
        subscription.TrialEnd.Should().NotBeNull();
        subscription.PlanId.Should().Be(pro.Id);

        var payment = await context.Payments.FirstAsync();
        payment.Status.Should().Be("pending");
        payment.Amount.Should().Be(499m);
    }

    [Fact]
    public async Task CreateCheckout_YearlyBilling_UsesYearlyPrice()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        // Act
        var result = await service.CreateCheckoutAsync(
            orgId, pro.Id, "yearly", "test@test.com", null, null, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CheckoutUrl.Should().Contain("amount=4999.00");

        var payment = await context.Payments.FirstAsync();
        payment.Amount.Should().Be(4999m);

        var subscription = await context.Subscriptions.FirstAsync();
        subscription.BillingCycle.Should().Be("yearly");
        subscription.CurrentPeriodEnd.Should().BeAfter(subscription.CurrentPeriodStart.AddMonths(11));
    }

    [Fact]
    public async Task CreateCheckout_InvalidPlanId_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        await SeedPlansAsync(context);

        // Act
        var result = await service.CreateCheckoutAsync(
            orgId, Guid.NewGuid(), "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCheckout_CancelsExistingActiveSubscription()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, premium) = await SeedPlansAsync(context);

        // Create a Pro subscription first
        await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Manually activate it (simulate ITN)
        var existingSub = await context.Subscriptions.FirstAsync();
        existingSub.Status = "active";
        await context.SaveChangesAsync();

        // Act — upgrade to Premium
        var result = await service.CreateCheckoutAsync(
            orgId, premium.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var subscriptions = await context.Subscriptions
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        subscriptions.Should().HaveCount(2);
        subscriptions[0].PlanId.Should().Be(premium.Id); // new one
        subscriptions[1].Status.Should().Be("cancelled"); // old one was cancelled
    }

    [Fact]
    public async Task CreateCheckout_NoTrialForReturningCustomer()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        // Create and activate a Pro subscription first (simulating previous use)
        await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        var firstSub = await context.Subscriptions.FirstAsync();
        firstSub.Status = "active";
        await context.SaveChangesAsync();

        // Cancel it
        await service.CancelSubscriptionAsync(orgId, userId, CancellationToken.None);

        // Act — re-subscribe to Pro (should not get a trial)
        var result = await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var newSub = await context.Subscriptions
            .OrderByDescending(s => s.CreatedAt)
            .FirstAsync();

        newSub.TrialEnd.Should().BeNull();
        newSub.Status.Should().Be("pending"); // No trial, straight to pending payment
    }

    // =========================================================================
    // ProcessItnAsync
    // =========================================================================

    [Fact]
    public async Task ProcessItn_ValidCompletePayment_ActivatesSubscription()
    {
        // Arrange
        var context = CreateDbContext();
        var payFast = CreatePayFastService();
        var service = CreateBillingService(context, payFast);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        var checkout = await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        var paymentId = checkout!.PaymentId;

        // Build ITN form data
        var formData = new Dictionary<string, string>
        {
            { "m_payment_id", paymentId.ToString() },
            { "payment_status", "COMPLETE" },
            { "amount_gross", "499.00" },
            { "pf_payment_id", "PF-12345" }
        };

        // Generate valid signature
        var signature = payFast.GenerateSignature(formData.Select(kv =>
            new KeyValuePair<string, string>(kv.Key, kv.Value)));
        formData["signature"] = signature;

        // Act
        var result = await service.ProcessItnAsync(formData, "127.0.0.1", CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var payment = await context.Payments.FirstAsync(p => p.Id == paymentId);
        payment.Status.Should().Be("completed");
        payment.PaidAt.Should().NotBeNull();
        payment.ExternalId.Should().Be("PF-12345");

        var subscription = await context.Subscriptions
            .FirstAsync(s => s.Id == checkout.SubscriptionId);
        subscription.Status.Should().Be("active");
        subscription.ExternalId.Should().Be("PF-12345");
    }

    [Fact]
    public async Task ProcessItn_InvalidSignature_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        var checkout = await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        var formData = new Dictionary<string, string>
        {
            { "m_payment_id", checkout!.PaymentId.ToString() },
            { "payment_status", "COMPLETE" },
            { "amount_gross", "499.00" },
            { "signature", "invalid_signature_value" }
        };

        // Act
        var result = await service.ProcessItnAsync(formData, "127.0.0.1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        // Payment should still be pending
        var payment = await context.Payments.FirstAsync(p => p.Id == checkout.PaymentId);
        payment.Status.Should().Be("pending");
    }

    [Fact]
    public async Task ProcessItn_AmountMismatch_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var payFast = CreatePayFastService();
        var service = CreateBillingService(context, payFast);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        var checkout = await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Wrong amount
        var formData = new Dictionary<string, string>
        {
            { "m_payment_id", checkout!.PaymentId.ToString() },
            { "payment_status", "COMPLETE" },
            { "amount_gross", "100.00" }
        };

        var signature = payFast.GenerateSignature(formData.Select(kv =>
            new KeyValuePair<string, string>(kv.Key, kv.Value)));
        formData["signature"] = signature;

        // Act
        var result = await service.ProcessItnAsync(formData, "127.0.0.1", CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessItn_CancelledPayment_CancelsSubscription()
    {
        // Arrange
        var context = CreateDbContext();
        var payFast = CreatePayFastService();
        var service = CreateBillingService(context, payFast);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        var checkout = await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        var formData = new Dictionary<string, string>
        {
            { "m_payment_id", checkout!.PaymentId.ToString() },
            { "payment_status", "CANCELLED" },
            { "amount_gross", "499.00" }
        };

        var signature = payFast.GenerateSignature(formData.Select(kv =>
            new KeyValuePair<string, string>(kv.Key, kv.Value)));
        formData["signature"] = signature;

        // Act
        var result = await service.ProcessItnAsync(formData, "127.0.0.1", CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var payment = await context.Payments.FirstAsync(p => p.Id == checkout.PaymentId);
        payment.Status.Should().Be("failed");

        var subscription = await context.Subscriptions.FirstAsync(s => s.Id == checkout.SubscriptionId);
        subscription.Status.Should().Be("cancelled");
        subscription.CancelledAt.Should().NotBeNull();
    }

    // =========================================================================
    // CancelSubscriptionAsync
    // =========================================================================

    [Fact]
    public async Task CancelSubscription_ActiveSubscription_CancelsSuccessfully()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (free, _, _) = await SeedPlansAsync(context);

        // Create and activate a subscription
        await service.CreateCheckoutAsync(
            orgId, free.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Act
        var result = await service.CancelSubscriptionAsync(orgId, userId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var subscription = await context.Subscriptions.FirstAsync();
        subscription.Status.Should().Be("cancelled");
        subscription.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CancelSubscription_NoActiveSubscription_ReturnsFalse()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        await SeedPlansAsync(context);

        // Act — no subscription exists
        var result = await service.CancelSubscriptionAsync(orgId, userId, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================================
    // HasFeatureAccessAsync
    // =========================================================================

    [Fact]
    public async Task HasFeatureAccess_NoSubscription_FreeTierRules()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, _) = await SeedSupplierOrgAsync(context);

        // Act & Assert — no subscription means free tier
        (await service.HasFeatureAccessAsync(orgId, "featured_listing", CancellationToken.None)).Should().BeFalse();
        (await service.HasFeatureAccessAsync(orgId, "analytics", CancellationToken.None)).Should().BeFalse();
        (await service.HasFeatureAccessAsync(orgId, "priority_support", CancellationToken.None)).Should().BeFalse();
        (await service.HasFeatureAccessAsync(orgId, "unlimited_leads", CancellationToken.None)).Should().BeFalse();
        (await service.HasFeatureAccessAsync(orgId, "unlimited_documents", CancellationToken.None)).Should().BeFalse();
    }

    [Fact]
    public async Task HasFeatureAccess_ProPlan_CorrectAccess()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        // Create and manually activate a Pro subscription
        var now = DateTime.UtcNow;
        context.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            PlanId = pro.Id,
            Status = "active",
            BillingCycle = "monthly",
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddMonths(1),
            CreatedAt = now,
            UpdatedAt = now
        });
        await context.SaveChangesAsync();

        // Act & Assert
        (await service.HasFeatureAccessAsync(orgId, "featured_listing", CancellationToken.None)).Should().BeTrue();
        (await service.HasFeatureAccessAsync(orgId, "analytics", CancellationToken.None)).Should().BeTrue();
        (await service.HasFeatureAccessAsync(orgId, "priority_support", CancellationToken.None)).Should().BeFalse(); // Pro doesn't get priority support
        (await service.HasFeatureAccessAsync(orgId, "unlimited_leads", CancellationToken.None)).Should().BeTrue(); // null MaxLeadsPerMonth = unlimited
    }

    [Fact]
    public async Task HasFeatureAccess_PremiumPlan_AllFeaturesEnabled()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, _, premium) = await SeedPlansAsync(context);

        // Create and manually activate a Premium subscription
        var now = DateTime.UtcNow;
        context.Subscriptions.Add(new Subscription
        {
            Id = Guid.NewGuid(),
            OrganizationId = orgId,
            PlanId = premium.Id,
            Status = "active",
            BillingCycle = "monthly",
            CurrentPeriodStart = now,
            CurrentPeriodEnd = now.AddMonths(1),
            CreatedAt = now,
            UpdatedAt = now
        });
        await context.SaveChangesAsync();

        // Act & Assert
        (await service.HasFeatureAccessAsync(orgId, "featured_listing", CancellationToken.None)).Should().BeTrue();
        (await service.HasFeatureAccessAsync(orgId, "analytics", CancellationToken.None)).Should().BeTrue();
        (await service.HasFeatureAccessAsync(orgId, "priority_support", CancellationToken.None)).Should().BeTrue();
        (await service.HasFeatureAccessAsync(orgId, "unlimited_leads", CancellationToken.None)).Should().BeTrue();
        (await service.HasFeatureAccessAsync(orgId, "unlimited_documents", CancellationToken.None)).Should().BeTrue();
    }

    // =========================================================================
    // GetPaymentHistoryAsync
    // =========================================================================

    [Fact]
    public async Task GetPaymentHistory_ReturnsPaymentsForOrganization()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (_, pro, _) = await SeedPlansAsync(context);

        // Create a subscription with payment
        await service.CreateCheckoutAsync(
            orgId, pro.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Act
        var result = await service.GetPaymentHistoryAsync(orgId, 1, 20, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Amount.Should().Be(499m);
        result.Items[0].Provider.Should().Be("payfast");
        result.Items[0].Status.Should().Be("pending");
        result.Total.Should().Be(1);
    }

    // =========================================================================
    // GetSubscriptionAsync
    // =========================================================================

    [Fact]
    public async Task GetSubscription_ReturnsActiveSubscription()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, userId) = await SeedSupplierOrgAsync(context);
        var (free, _, _) = await SeedPlansAsync(context);

        await service.CreateCheckoutAsync(
            orgId, free.Id, "monthly", "test@test.com", null, null, userId, CancellationToken.None);

        // Act
        var subscription = await service.GetSubscriptionAsync(orgId, CancellationToken.None);

        // Assert
        subscription.Should().NotBeNull();
        subscription!.PlanName.Should().Be("free");
        subscription.PlanDisplayName.Should().Be("Free");
        subscription.Status.Should().Be("active");
    }

    [Fact]
    public async Task GetSubscription_NoActiveSubscription_ReturnsNull()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateBillingService(context);
        var (orgId, _) = await SeedSupplierOrgAsync(context);

        // Act
        var subscription = await service.GetSubscriptionAsync(orgId, CancellationToken.None);

        // Assert
        subscription.Should().BeNull();
    }
}
