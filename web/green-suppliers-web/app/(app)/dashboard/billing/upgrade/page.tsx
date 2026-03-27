"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import { apiGet, apiPost } from "@/lib/api-client";
import type { PlanDto, CheckoutResult, SubscriptionDto } from "@/lib/types";
import { apiGetAuth } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Check,
  X,
  ArrowLeft,
  ArrowRight,
  Sparkles,
  Loader2,
  AlertTriangle,
  Shield,
} from "lucide-react";
import { toast } from "sonner";

/* -------------------------------------------------------------------------- */
/*  Helpers                                                                    */
/* -------------------------------------------------------------------------- */

function formatZAR(amount: number): string {
  if (amount === 0) return "Free";
  return `R${amount.toLocaleString("en-ZA")}`;
}

/* -------------------------------------------------------------------------- */
/*  Page                                                                       */
/* -------------------------------------------------------------------------- */

export default function UpgradePage() {
  const { token } = useAuth();
  const router = useRouter();

  const [plans, setPlans] = useState<PlanDto[]>([]);
  const [currentSub, setCurrentSub] = useState<SubscriptionDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Selection state
  const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
  const [billingCycle, setBillingCycle] = useState<"monthly" | "yearly">("monthly");
  const [submitting, setSubmitting] = useState(false);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);

    const plansRes = await apiGet<PlanDto[]>("/plans", { revalidate: 60 });

    if (!plansRes.success || !plansRes.data) {
      setError(plansRes.error?.message ?? "Failed to load plans");
      setLoading(false);
      return;
    }

    const sorted = [...plansRes.data].sort((a, b) => a.sortOrder - b.sortOrder);
    setPlans(sorted);

    // Fetch current subscription if authenticated
    if (token) {
      const subRes = await apiGetAuth<SubscriptionDto | null>(
        "/supplier/billing/subscription",
        token
      );
      if (subRes.success && subRes.data) {
        setCurrentSub(subRes.data);
      }
    }

    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  // Auto-select Pro plan by default once plans load
  useEffect(() => {
    if (plans.length > 0 && selectedPlanId === null) {
      const pro = plans.find((p) => p.name === "pro");
      if (pro) {
        setSelectedPlanId(pro.id);
      } else if (plans.length > 1) {
        setSelectedPlanId(plans[1].id);
      }
    }
  }, [plans, selectedPlanId]);

  const selectedPlan = plans.find((p) => p.id === selectedPlanId) ?? null;
  const selectedPrice =
    selectedPlan
      ? billingCycle === "yearly"
        ? selectedPlan.priceYearly
        : selectedPlan.priceMonthly
      : 0;
  const isFreePlan = selectedPlan?.priceMonthly === 0;
  const isCurrentPlan =
    currentSub && selectedPlan && currentSub.planId === selectedPlan.id;

  async function handleCheckout() {
    if (!selectedPlanId || !token) return;

    // Free plan: activate directly
    if (isFreePlan) {
      setSubmitting(true);
      const res = await apiPost<CheckoutResult>(
        "/supplier/billing/checkout",
        { planId: selectedPlanId, billingCycle: "monthly" },
        token
      );
      setSubmitting(false);

      if (res.success) {
        toast.success("Free plan activated successfully!");
        router.push("/dashboard/billing");
      } else {
        toast.error(res.error?.message ?? "Failed to activate plan");
      }
      return;
    }

    // Paid plan: create checkout and redirect
    setSubmitting(true);
    const res = await apiPost<CheckoutResult>(
      "/supplier/billing/checkout",
      { planId: selectedPlanId, billingCycle },
      token
    );
    setSubmitting(false);

    if (res.success && res.data) {
      if (res.data.checkoutUrl) {
        window.location.href = res.data.checkoutUrl;
      } else {
        // Edge case: paid plan returned no URL (should not happen)
        toast.success("Plan activated!");
        router.push("/dashboard/billing");
      }
    } else {
      toast.error(res.error?.message ?? "Failed to create checkout session");
    }
  }

  if (loading) {
    return (
      <div role="status" aria-label="Loading plans" className="space-y-6">
        <Skeleton className="h-8 w-64" />
        <div className="grid gap-4 sm:grid-cols-3">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-72 rounded-2xl" />
          ))}
        </div>
        <span className="sr-only">Loading plans</span>
      </div>
    );
  }

  if (error) {
    return (
      <div role="alert" className="flex flex-col items-center justify-center gap-4 py-20">
        <AlertTriangle className="h-10 w-10 text-destructive" aria-hidden="true" />
        <p className="text-sm text-muted-foreground">{error}</p>
        <Button variant="outline" onClick={() => fetchData()}>
          Try Again
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <Link
          href="/dashboard/billing"
          className="mb-3 inline-flex items-center gap-1 text-sm text-muted-foreground transition-colors hover:text-foreground"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Billing
        </Link>
        <h1 className="text-2xl font-bold tracking-tight">Choose Your Plan</h1>
        <p className="text-sm text-muted-foreground">
          Select a plan and billing cycle. Paid plans include a 14-day free trial.
        </p>
      </div>

      {/* Monthly / Yearly toggle */}
      <div className="flex items-center justify-center gap-3 rounded-2xl border bg-white p-4 shadow-sm">
        <button
          onClick={() => setBillingCycle("monthly")}
          className={`rounded-lg px-4 py-2 text-sm font-medium transition-colors ${
            billingCycle === "monthly"
              ? "bg-brand-green text-white"
              : "text-muted-foreground hover:text-foreground"
          }`}
        >
          Monthly
        </button>
        <button
          onClick={() => setBillingCycle("yearly")}
          className={`rounded-lg px-4 py-2 text-sm font-medium transition-colors ${
            billingCycle === "yearly"
              ? "bg-brand-green text-white"
              : "text-muted-foreground hover:text-foreground"
          }`}
        >
          Yearly
          <span className="ml-1.5 inline-flex rounded-full bg-brand-green-light px-2 py-0.5 text-[10px] font-semibold text-brand-green">
            Save ~17%
          </span>
        </button>
      </div>

      {/* Plan cards */}
      <div className="grid gap-4 sm:grid-cols-3">
        {plans.map((plan) => {
          const isPro = plan.name === "pro";
          const isSelected = plan.id === selectedPlanId;
          const isCurrent = currentSub?.planId === plan.id;
          const price =
            billingCycle === "yearly" ? plan.priceYearly : plan.priceMonthly;
          const isFree = plan.priceMonthly === 0;

          return (
            <button
              key={plan.id}
              type="button"
              onClick={() => setSelectedPlanId(plan.id)}
              aria-pressed={isSelected}
              className={`relative flex flex-col rounded-2xl border p-5 text-left transition-all ${
                isSelected
                  ? "border-brand-green ring-2 ring-brand-green/20 bg-white shadow-md"
                  : "border-gray-200 bg-white hover:border-gray-300 hover:shadow-sm"
              }`}
            >
              {/* Badges */}
              {isPro && (
                <span className="absolute -top-2.5 left-4 inline-flex items-center gap-1 rounded-full bg-brand-green px-3 py-0.5 text-[10px] font-semibold text-white">
                  <Sparkles className="h-2.5 w-2.5" />
                  Most Popular
                </span>
              )}
              {isCurrent && (
                <span className="absolute -top-2.5 right-4 inline-flex items-center gap-1 rounded-full bg-blue-500 px-3 py-0.5 text-[10px] font-semibold text-white">
                  Current Plan
                </span>
              )}

              <h3 className="text-lg font-bold text-foreground">{plan.displayName}</h3>

              {/* Trial badge */}
              {plan.trialDays > 0 && !isFree && (
                <span className="mt-1 inline-flex w-fit items-center gap-1 rounded-full bg-blue-50 px-2 py-0.5 text-[10px] font-medium text-blue-700">
                  <Shield className="h-2.5 w-2.5" />
                  {plan.trialDays}-day free trial
                </span>
              )}

              {/* Price */}
              <div className="mt-3 mb-4">
                <span className="text-3xl font-extrabold tracking-tight text-foreground">
                  {formatZAR(isFree ? 0 : price)}
                </span>
                {!isFree && (
                  <span className="text-sm text-muted-foreground">
                    /{billingCycle === "yearly" ? "year" : "month"}
                  </span>
                )}
                {billingCycle === "yearly" && !isFree && (
                  <p className="mt-0.5 text-xs text-brand-green font-medium">
                    {formatZAR(Math.round(price / 12))}/mo
                  </p>
                )}
              </div>

              {/* Features */}
              <ul className="flex-1 space-y-2 text-sm">
                <PlanFeature included>
                  {plan.maxLeadsPerMonth != null
                    ? `${plan.maxLeadsPerMonth} leads/mo`
                    : "Unlimited leads"}
                </PlanFeature>
                <PlanFeature included>
                  {plan.maxDocuments != null
                    ? `${plan.maxDocuments} documents`
                    : "Unlimited documents"}
                </PlanFeature>
                <PlanFeature included={plan.featuredListing}>Featured listing</PlanFeature>
                <PlanFeature included={plan.analyticsAccess}>Analytics</PlanFeature>
                <PlanFeature included={plan.prioritySupport}>Priority support</PlanFeature>
              </ul>

              {/* Selection indicator */}
              <div
                className={`mt-4 flex h-8 items-center justify-center rounded-lg border text-xs font-semibold transition-colors ${
                  isSelected
                    ? "border-brand-green bg-brand-green text-white"
                    : "border-gray-200 text-muted-foreground"
                }`}
              >
                {isSelected ? "Selected" : "Select"}
              </div>
            </button>
          );
        })}
      </div>

      {/* Order summary */}
      {selectedPlan && (
        <div className="rounded-2xl border bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Order Summary
          </h2>
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-lg font-bold text-foreground">
                {selectedPlan.displayName} Plan
              </p>
              <p className="text-sm text-muted-foreground capitalize">
                {isFreePlan ? "Free forever" : `${billingCycle} billing`}
              </p>
              {selectedPlan.trialDays > 0 && !isFreePlan && (
                <p className="mt-1 text-sm text-blue-600">
                  Includes {selectedPlan.trialDays}-day free trial
                </p>
              )}
            </div>
            <div className="text-right">
              <p className="text-2xl font-extrabold text-foreground">
                {formatZAR(isFreePlan ? 0 : selectedPrice)}
              </p>
              {!isFreePlan && (
                <p className="text-xs text-muted-foreground">
                  {billingCycle === "yearly"
                    ? `${formatZAR(Math.round(selectedPrice / 12))}/mo billed yearly`
                    : "billed monthly"}
                </p>
              )}
            </div>
          </div>

          <div className="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
            <Link href="/dashboard/billing">
              <Button variant="outline">Cancel</Button>
            </Link>
            <Button
              onClick={handleCheckout}
              disabled={submitting || !!isCurrentPlan}
              className="gap-1.5"
            >
              {submitting ? (
                <>
                  <Loader2 className="h-4 w-4 animate-spin" />
                  Processing...
                </>
              ) : isCurrentPlan ? (
                "Already on this plan"
              ) : isFreePlan ? (
                <>
                  Activate Free Plan
                  <ArrowRight className="h-4 w-4" />
                </>
              ) : (
                <>
                  Proceed to Payment
                  <ArrowRight className="h-4 w-4" />
                </>
              )}
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}

/* -------------------------------------------------------------------------- */
/*  Plan feature list item                                                     */
/* -------------------------------------------------------------------------- */

function PlanFeature({
  included,
  children,
}: {
  included: boolean;
  children: React.ReactNode;
}) {
  return (
    <li className="flex items-center gap-2">
      {included ? (
        <Check className="h-3.5 w-3.5 shrink-0 text-brand-green" />
      ) : (
        <X className="h-3.5 w-3.5 shrink-0 text-gray-300" />
      )}
      <span className={included ? "text-foreground" : "text-muted-foreground line-through"}>
        {children}
      </span>
    </li>
  );
}
