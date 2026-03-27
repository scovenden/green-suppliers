import { Metadata } from "next";
import Link from "next/link";
import { apiGet } from "@/lib/api-client";
import type { PlanDto } from "@/lib/types";
import {
  Check,
  X,
  Sparkles,
  Shield,
  Zap,
  HelpCircle,
  ArrowRight,
} from "lucide-react";

export const metadata: Metadata = {
  title: "Pricing - Green Suppliers",
  description:
    "Choose the right plan for your business. List your green supplier profile for free, or upgrade to Pro or Premium for featured listings, unlimited leads, and priority support.",
};

/* -------------------------------------------------------------------------- */
/*  Static fallback plans (used when the API is unreachable during SSR)        */
/* -------------------------------------------------------------------------- */

const FALLBACK_PLANS: PlanDto[] = [
  {
    id: "free",
    name: "free",
    displayName: "Free",
    priceMonthly: 0,
    priceYearly: 0,
    currency: "ZAR",
    maxLeadsPerMonth: 5,
    maxDocuments: 3,
    featuredListing: false,
    analyticsAccess: false,
    prioritySupport: false,
    trialDays: 0,
    sortOrder: 1,
  },
  {
    id: "pro",
    name: "pro",
    displayName: "Pro",
    priceMonthly: 499,
    priceYearly: 4990,
    currency: "ZAR",
    maxLeadsPerMonth: null,
    maxDocuments: 20,
    featuredListing: true,
    analyticsAccess: true,
    prioritySupport: false,
    trialDays: 14,
    sortOrder: 2,
  },
  {
    id: "premium",
    name: "premium",
    displayName: "Premium",
    priceMonthly: 999,
    priceYearly: 9990,
    currency: "ZAR",
    maxLeadsPerMonth: null,
    maxDocuments: null,
    featuredListing: true,
    analyticsAccess: true,
    prioritySupport: true,
    trialDays: 14,
    sortOrder: 3,
  },
];

/* -------------------------------------------------------------------------- */
/*  Feature comparison rows                                                    */
/* -------------------------------------------------------------------------- */

interface FeatureRow {
  label: string;
  free: string | boolean;
  pro: string | boolean;
  premium: string | boolean;
}

function buildFeatureRows(plans: PlanDto[]): FeatureRow[] {
  const free = plans.find((p) => p.name === "free");
  const pro = plans.find((p) => p.name === "pro");
  const premium = plans.find((p) => p.name === "premium");

  return [
    {
      label: "Supplier Profile",
      free: true,
      pro: true,
      premium: true,
    },
    {
      label: "Leads per Month",
      free: free?.maxLeadsPerMonth != null ? `${free.maxLeadsPerMonth}` : "Unlimited",
      pro: pro?.maxLeadsPerMonth != null ? `${pro.maxLeadsPerMonth}` : "Unlimited",
      premium: premium?.maxLeadsPerMonth != null ? `${premium.maxLeadsPerMonth}` : "Unlimited",
    },
    {
      label: "Document Uploads",
      free: free?.maxDocuments != null ? `${free.maxDocuments}` : "Unlimited",
      pro: pro?.maxDocuments != null ? `${pro.maxDocuments}` : "Unlimited",
      premium: premium?.maxDocuments != null ? `${premium.maxDocuments}` : "Unlimited",
    },
    {
      label: "Featured Listing",
      free: free?.featuredListing ?? false,
      pro: pro?.featuredListing ?? false,
      premium: premium?.featuredListing ?? false,
    },
    {
      label: "Analytics Dashboard",
      free: free?.analyticsAccess ?? false,
      pro: pro?.analyticsAccess ?? false,
      premium: premium?.analyticsAccess ?? false,
    },
    {
      label: "Priority Support",
      free: free?.prioritySupport ?? false,
      pro: pro?.prioritySupport ?? false,
      premium: premium?.prioritySupport ?? false,
    },
    {
      label: "ESG Verification Badge",
      free: true,
      pro: true,
      premium: true,
    },
    {
      label: "Certification Tracking",
      free: true,
      pro: true,
      premium: true,
    },
  ];
}

/* -------------------------------------------------------------------------- */
/*  FAQ data                                                                   */
/* -------------------------------------------------------------------------- */

const FAQ_ITEMS = [
  {
    question: "Can I try Pro or Premium before committing?",
    answer:
      "Yes. Both Pro and Premium plans include a 14-day free trial. You won't be charged until the trial ends, and you can cancel at any time during the trial period.",
  },
  {
    question: "How does billing work?",
    answer:
      "We offer both monthly and yearly billing. Yearly billing saves you roughly 17% compared to monthly. All prices are in South African Rand (ZAR) and processed securely via PayFast.",
  },
  {
    question: "Can I upgrade or downgrade my plan?",
    answer:
      "Absolutely. You can upgrade at any time from your billing dashboard. When you upgrade, the new plan takes effect immediately. Downgrades take effect at the end of your current billing period.",
  },
  {
    question: "What happens when I cancel?",
    answer:
      "When you cancel, your subscription remains active until the end of the current billing period. After that, your profile reverts to the Free plan with limited features.",
  },
  {
    question: "Is the Free plan really free?",
    answer:
      "Yes. The Free plan gives you a verified supplier profile, up to 5 leads per month, and 3 document uploads. No credit card required.",
  },
  {
    question: "Do you offer discounts for NGOs or startups?",
    answer:
      "We offer special pricing for registered NGOs and early-stage green startups. Contact us at hello@greensuppliers.co.za to discuss your needs.",
  },
];

/* -------------------------------------------------------------------------- */
/*  Helper: format ZAR amounts                                                 */
/* -------------------------------------------------------------------------- */

function formatZAR(amount: number): string {
  if (amount === 0) return "Free";
  return `R${amount.toLocaleString("en-ZA")}`;
}

/* -------------------------------------------------------------------------- */
/*  Plan card component                                                        */
/* -------------------------------------------------------------------------- */

function PlanCard({
  plan,
  highlighted,
}: {
  plan: PlanDto;
  highlighted: boolean;
}) {
  const isFreePlan = plan.priceMonthly === 0;
  const yearlyMonthly = plan.priceYearly > 0 ? Math.round(plan.priceYearly / 12) : 0;
  const monthlySaving =
    plan.priceMonthly > 0 && yearlyMonthly > 0
      ? plan.priceMonthly - yearlyMonthly
      : 0;

  return (
    <div
      className={`relative flex flex-col rounded-2xl border bg-white p-6 shadow-sm transition-shadow hover:shadow-md ${
        highlighted
          ? "border-brand-green ring-2 ring-brand-green/20 shadow-lg"
          : "border-gray-200"
      }`}
    >
      {/* Most Popular badge */}
      {highlighted && (
        <div className="absolute -top-3.5 left-1/2 -translate-x-1/2">
          <span className="inline-flex items-center gap-1 rounded-full bg-brand-green px-4 py-1 text-xs font-semibold text-white shadow-sm">
            <Sparkles className="h-3 w-3" />
            Most Popular
          </span>
        </div>
      )}

      {/* Plan name */}
      <div className="mb-4">
        <h3 className="text-xl font-bold text-foreground">{plan.displayName}</h3>
        {plan.trialDays > 0 && (
          <span className="mt-1 inline-flex items-center rounded-full bg-blue-50 px-2.5 py-0.5 text-xs font-medium text-blue-700">
            {plan.trialDays}-day free trial
          </span>
        )}
      </div>

      {/* Price */}
      <div className="mb-6">
        <div className="flex items-baseline gap-1">
          <span className="text-4xl font-extrabold tracking-tight text-foreground">
            {formatZAR(plan.priceMonthly)}
          </span>
          {!isFreePlan && (
            <span className="text-sm text-muted-foreground">/month</span>
          )}
        </div>
        {monthlySaving > 0 && (
          <p className="mt-1 text-sm text-brand-green font-medium">
            or {formatZAR(yearlyMonthly)}/mo billed yearly (save R{monthlySaving}/mo)
          </p>
        )}
        {isFreePlan && (
          <p className="mt-1 text-sm text-muted-foreground">
            No credit card required
          </p>
        )}
      </div>

      {/* Feature list */}
      <ul className="mb-8 flex-1 space-y-3">
        <FeatureItem included>Verified Supplier Profile</FeatureItem>
        <FeatureItem included>
          {plan.maxLeadsPerMonth != null
            ? `${plan.maxLeadsPerMonth} leads/month`
            : "Unlimited leads"}
        </FeatureItem>
        <FeatureItem included>
          {plan.maxDocuments != null
            ? `${plan.maxDocuments} document uploads`
            : "Unlimited documents"}
        </FeatureItem>
        <FeatureItem included={plan.featuredListing}>Featured listing</FeatureItem>
        <FeatureItem included={plan.analyticsAccess}>Analytics dashboard</FeatureItem>
        <FeatureItem included={plan.prioritySupport}>Priority support</FeatureItem>
      </ul>

      {/* CTA */}
      <Link
        href={isFreePlan ? "/get-listed" : "/dashboard/billing/upgrade"}
        className={`flex items-center justify-center gap-2 rounded-lg px-4 py-3 text-sm font-semibold transition-colors ${
          highlighted
            ? "bg-brand-green text-white hover:bg-brand-green-hover"
            : isFreePlan
            ? "border border-brand-green text-brand-green hover:bg-brand-green-light"
            : "bg-brand-dark text-white hover:bg-brand-dark/90"
        }`}
      >
        {isFreePlan ? "Get Started Free" : `Start ${plan.trialDays}-day trial`}
        <ArrowRight className="h-4 w-4" />
      </Link>
    </div>
  );
}

function FeatureItem({
  included,
  children,
}: {
  included: boolean;
  children: React.ReactNode;
}) {
  return (
    <li className="flex items-start gap-2 text-sm">
      {included ? (
        <Check className="mt-0.5 h-4 w-4 shrink-0 text-brand-green" />
      ) : (
        <X className="mt-0.5 h-4 w-4 shrink-0 text-gray-300" />
      )}
      <span className={included ? "text-foreground" : "text-muted-foreground line-through"}>
        {children}
      </span>
    </li>
  );
}

/* -------------------------------------------------------------------------- */
/*  Feature comparison table                                                   */
/* -------------------------------------------------------------------------- */

function FeatureComparisonTable({ plans }: { plans: PlanDto[] }) {
  const rows = buildFeatureRows(plans);

  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="border-b">
            <th className="pb-3 pr-4 text-left font-semibold text-foreground">
              Feature
            </th>
            <th className="pb-3 px-4 text-center font-semibold text-foreground">
              Free
            </th>
            <th className="pb-3 px-4 text-center font-semibold text-brand-green">
              Pro
            </th>
            <th className="pb-3 pl-4 text-center font-semibold text-foreground">
              Premium
            </th>
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={row.label} className="border-b last:border-0">
              <td className="py-3 pr-4 font-medium text-foreground">
                {row.label}
              </td>
              <ComparisonCell value={row.free} />
              <ComparisonCell value={row.pro} highlighted />
              <ComparisonCell value={row.premium} />
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function ComparisonCell({
  value,
  highlighted,
}: {
  value: string | boolean;
  highlighted?: boolean;
}) {
  return (
    <td
      className={`py-3 px-4 text-center ${
        highlighted ? "bg-brand-green-light/50" : ""
      }`}
    >
      {typeof value === "boolean" ? (
        value ? (
          <Check className="mx-auto h-4 w-4 text-brand-green" />
        ) : (
          <X className="mx-auto h-4 w-4 text-gray-300" />
        )
      ) : (
        <span className="font-medium text-foreground">{value}</span>
      )}
    </td>
  );
}

/* -------------------------------------------------------------------------- */
/*  FAQ Section                                                                */
/* -------------------------------------------------------------------------- */

function FAQSection() {
  return (
    <section className="mx-auto max-w-3xl">
      <div className="mb-8 text-center">
        <h2 className="text-2xl font-bold tracking-tight text-foreground sm:text-3xl">
          Frequently Asked Questions
        </h2>
        <p className="mt-2 text-muted-foreground">
          Everything you need to know about our plans
        </p>
      </div>
      <div className="divide-y rounded-2xl border bg-white shadow-sm">
        {FAQ_ITEMS.map((item) => (
          <details
            key={item.question}
            className="group px-6 py-5"
          >
            <summary className="flex cursor-pointer items-center justify-between gap-4 text-sm font-semibold text-foreground marker:content-[''] [&::-webkit-details-marker]:hidden">
              <span className="flex items-center gap-2">
                <HelpCircle className="h-4 w-4 shrink-0 text-brand-green" />
                {item.question}
              </span>
              <span className="shrink-0 text-muted-foreground transition-transform group-open:rotate-180">
                <svg
                  className="h-4 w-4"
                  fill="none"
                  viewBox="0 0 24 24"
                  stroke="currentColor"
                  strokeWidth={2}
                >
                  <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
                </svg>
              </span>
            </summary>
            <p className="mt-3 pl-6 text-sm leading-relaxed text-muted-foreground">
              {item.answer}
            </p>
          </details>
        ))}
      </div>
    </section>
  );
}

/* -------------------------------------------------------------------------- */
/*  Page component (SSR)                                                       */
/* -------------------------------------------------------------------------- */

export default async function PricingPage() {
  const res = await apiGet<PlanDto[]>("/plans", { revalidate: 300 });
  const plans =
    res.success && res.data && res.data.length > 0
      ? res.data
      : FALLBACK_PLANS;

  // Sort by sortOrder
  const sortedPlans = [...plans].sort((a, b) => a.sortOrder - b.sortOrder);

  return (
    <div className="bg-white">
      {/* Hero */}
      <section className="relative overflow-hidden bg-gradient-to-b from-brand-green-light to-white px-4 pb-16 pt-20 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-4xl text-center">
          <div className="mb-4 inline-flex items-center gap-2 rounded-full bg-brand-green/10 px-4 py-1.5 text-sm font-medium text-brand-green">
            <Shield className="h-4 w-4" />
            Transparent Pricing
          </div>
          <h1 className="text-3xl font-extrabold tracking-tight text-foreground sm:text-4xl lg:text-5xl">
            Choose the right plan for
            <br />
            <span className="text-brand-green">your green business</span>
          </h1>
          <p className="mx-auto mt-4 max-w-2xl text-lg text-muted-foreground">
            Start for free and upgrade when you need more leads, analytics, and visibility.
            All plans include a verified supplier profile and ESG badges.
          </p>
        </div>
      </section>

      {/* Plan cards */}
      <section className="px-4 pb-16 sm:px-6 lg:px-8">
        <div className="mx-auto grid max-w-5xl gap-6 sm:gap-8 md:grid-cols-3">
          {sortedPlans.map((plan) => (
            <PlanCard
              key={plan.id}
              plan={plan}
              highlighted={plan.name === "pro"}
            />
          ))}
        </div>
      </section>

      {/* Feature comparison */}
      <section className="bg-gray-50 px-4 py-16 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-4xl">
          <div className="mb-10 text-center">
            <h2 className="text-2xl font-bold tracking-tight text-foreground sm:text-3xl">
              Compare Plans
            </h2>
            <p className="mt-2 text-muted-foreground">
              See exactly what each plan includes
            </p>
          </div>
          <div className="rounded-2xl border bg-white p-6 shadow-sm sm:p-8">
            <FeatureComparisonTable plans={sortedPlans} />
          </div>
        </div>
      </section>

      {/* Trust indicators */}
      <section className="px-4 py-16 sm:px-6 lg:px-8">
        <div className="mx-auto grid max-w-4xl gap-8 sm:grid-cols-3">
          <div className="flex flex-col items-center text-center">
            <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-brand-green/10">
              <Shield className="h-6 w-6 text-brand-green" />
            </div>
            <h3 className="mb-1 font-semibold text-foreground">Secure Payments</h3>
            <p className="text-sm text-muted-foreground">
              All transactions processed securely via PayFast with 256-bit encryption.
            </p>
          </div>
          <div className="flex flex-col items-center text-center">
            <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-brand-green/10">
              <Zap className="h-6 w-6 text-brand-green" />
            </div>
            <h3 className="mb-1 font-semibold text-foreground">Cancel Anytime</h3>
            <p className="text-sm text-muted-foreground">
              No lock-in contracts. Cancel your subscription at any time from your dashboard.
            </p>
          </div>
          <div className="flex flex-col items-center text-center">
            <div className="mb-3 flex h-12 w-12 items-center justify-center rounded-xl bg-brand-green/10">
              <Sparkles className="h-6 w-6 text-brand-green" />
            </div>
            <h3 className="mb-1 font-semibold text-foreground">14-Day Free Trial</h3>
            <p className="text-sm text-muted-foreground">
              Try Pro or Premium risk-free. No credit card required to start your trial.
            </p>
          </div>
        </div>
      </section>

      {/* FAQ */}
      <section className="bg-gray-50 px-4 py-16 sm:px-6 lg:px-8">
        <FAQSection />
      </section>

      {/* CTA */}
      <section className="px-4 py-16 sm:px-6 lg:px-8">
        <div className="mx-auto max-w-3xl rounded-2xl bg-brand-dark p-8 text-center sm:p-12">
          <h2 className="text-2xl font-bold text-white sm:text-3xl">
            Ready to grow your green business?
          </h2>
          <p className="mx-auto mt-3 max-w-xl text-sm text-white/70">
            Join hundreds of verified green suppliers already connecting with enterprise buyers across South Africa.
          </p>
          <div className="mt-6 flex flex-col items-center gap-3 sm:flex-row sm:justify-center">
            <Link
              href="/get-listed"
              className="inline-flex items-center gap-2 rounded-lg bg-brand-green px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-brand-green-hover"
            >
              Get Listed Free
              <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              href="/contact"
              className="inline-flex items-center gap-2 rounded-lg border border-white/20 px-6 py-3 text-sm font-semibold text-white transition-colors hover:bg-white/10"
            >
              Talk to Sales
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
