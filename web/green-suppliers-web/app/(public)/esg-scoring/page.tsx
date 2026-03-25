import type { Metadata } from "next";
import Link from "next/link";
import {
  BarChart3,
  CheckCircle,
  ArrowRight,
  Leaf,
  Zap,
  Recycle,
  FileText,
  Droplets,
  Package,
  Shield,
} from "lucide-react";

export const metadata: Metadata = {
  title: "ESG Scoring Methodology | Green Suppliers",
  description:
    "Understand how Green Suppliers calculates ESG scores. Our transparent, rules-based methodology evaluates renewable energy, waste recycling, carbon reporting, and certifications.",
};

const scoringFactors = [
  {
    icon: Zap,
    title: "Renewable Energy Usage",
    description:
      "Percentage of energy sourced from renewable sources. Higher usage leads to a higher score and unlocks Silver tier and above.",
    weight: "High",
    color: "text-amber-500",
    bg: "bg-amber-50",
  },
  {
    icon: Recycle,
    title: "Waste Recycling Rate",
    description:
      "Percentage of waste that is recycled or diverted from landfill. Required at 70%+ for Platinum tier.",
    weight: "High",
    color: "text-emerald-500",
    bg: "bg-emerald-50",
  },
  {
    icon: FileText,
    title: "Carbon Reporting",
    description:
      "Whether the supplier actively tracks and reports carbon emissions. Required for Gold and Platinum tiers.",
    weight: "Medium",
    color: "text-blue-500",
    bg: "bg-blue-50",
  },
  {
    icon: Shield,
    title: "Certifications",
    description:
      "Number of valid, non-expired certifications (ISO 14001, B-Corp, FSC, GBCSA, etc.). More certifications unlock higher tiers.",
    weight: "High",
    color: "text-brand-green",
    bg: "bg-brand-green-light",
  },
  {
    icon: Droplets,
    title: "Water Management",
    description:
      "Active water conservation, recycling, or treatment programmes. A positive indicator in the overall ESG assessment.",
    weight: "Low",
    color: "text-blue-600",
    bg: "bg-blue-50",
  },
  {
    icon: Package,
    title: "Sustainable Packaging",
    description:
      "Use of biodegradable, recyclable, or sustainably sourced packaging materials. A positive indicator in the overall assessment.",
    weight: "Low",
    color: "text-green-600",
    bg: "bg-green-50",
  },
];

export default function EsgScoringPage() {
  return (
    <div className="min-h-screen bg-gray-50/50">
      {/* Breadcrumbs */}
      <div className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-3 sm:px-6 lg:px-8">
          <nav className="flex items-center gap-2 text-sm text-gray-500">
            <Link
              href="/"
              className="transition-colors hover:text-brand-green"
            >
              Home
            </Link>
            <span>/</span>
            <span className="font-medium text-gray-900">ESG Scoring</span>
          </nav>
        </div>
      </div>

      {/* Hero */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 sm:py-16 lg:px-8">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mx-auto mb-5 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-green-light">
              <BarChart3 className="h-7 w-7 text-brand-green" />
            </div>
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              ESG Scoring Methodology
            </h1>
            <p className="mx-auto mt-4 max-w-2xl text-lg leading-relaxed text-gray-500">
              Our scoring system is transparent, rules-based, and recalculated
              automatically. Every supplier is measured against the same
              criteria.
            </p>
          </div>
        </div>
      </section>

      {/* How it works */}
      <section className="py-16">
        <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-2xl font-extrabold tracking-tight text-gray-900">
            How ESG Scoring Works
          </h2>
          <div className="mt-4 space-y-4 text-base leading-relaxed text-gray-600">
            <p>
              Every supplier on Green Suppliers is assessed across multiple
              sustainability dimensions. The ESG score is a composite measure
              that factors in certifications, energy sourcing, waste management,
              and reporting practices.
            </p>
            <p>
              Scores are recalculated <strong>nightly</strong> and whenever a
              supplier&apos;s certification status changes (e.g., a new
              certification is uploaded or an existing one expires). This
              ensures scores always reflect the latest data.
            </p>
            <p>
              The resulting score places each supplier into one of four tiers:
              Bronze, Silver, Gold, or Platinum. The tier requirements are
              published openly so suppliers know exactly what they need to
              achieve the next level.
            </p>
          </div>
        </div>
      </section>

      {/* Scoring Factors */}
      <section className="bg-white py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
              What We Measure
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-base text-gray-500">
              Six key dimensions make up the ESG assessment
            </p>
          </div>
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {scoringFactors.map((factor) => {
              const Icon = factor.icon;
              return (
                <div
                  key={factor.title}
                  className="flex flex-col gap-3 rounded-2xl border border-gray-100 bg-gray-50/50 p-6"
                >
                  <div className="flex items-center gap-3">
                    <div
                      className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${factor.bg}`}
                    >
                      <Icon className={`h-5 w-5 ${factor.color}`} />
                    </div>
                    <div>
                      <h3 className="text-sm font-semibold text-gray-900">
                        {factor.title}
                      </h3>
                      <span className="text-xs text-gray-400">
                        Weight: {factor.weight}
                      </span>
                    </div>
                  </div>
                  <p className="text-sm leading-relaxed text-gray-500">
                    {factor.description}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Tier requirements */}
      <section className="py-16">
        <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-2xl font-extrabold tracking-tight text-gray-900">
            Tier Requirements
          </h2>
          <div className="mt-6 space-y-4">
            {[
              {
                tier: "Bronze",
                color: "bg-gradient-to-r from-amber-700 to-amber-800",
                textColor: "text-amber-800",
                reqs: ["All required company fields filled"],
              },
              {
                tier: "Silver",
                color: "bg-gradient-to-r from-gray-400 to-gray-500",
                textColor: "text-gray-600",
                reqs: [
                  "At least 1 valid (non-expired) certification",
                  "Renewable energy usage >= 20%",
                ],
              },
              {
                tier: "Gold",
                color: "bg-gradient-to-r from-amber-500 to-amber-600",
                textColor: "text-amber-700",
                reqs: [
                  "At least 2 valid certifications",
                  "Renewable energy usage >= 50%",
                  "Carbon reporting: Yes",
                ],
              },
              {
                tier: "Platinum",
                color: "bg-gradient-to-r from-lime-600 to-green-700",
                textColor: "text-green-700",
                reqs: [
                  "At least 3 valid certifications",
                  "Renewable energy usage >= 70%",
                  "Waste recycling >= 70%",
                  "Carbon reporting: Yes",
                ],
              },
            ].map((item) => (
              <div
                key={item.tier}
                className="flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm"
              >
                <div className={`${item.color} px-5 py-3`}>
                  <h3 className="text-base font-bold text-white">
                    {item.tier}
                  </h3>
                </div>
                <ul className="flex flex-col gap-2 p-5">
                  {item.reqs.map((req) => (
                    <li
                      key={req}
                      className="flex items-start gap-2 text-sm text-gray-600"
                    >
                      <CheckCircle
                        className={`mt-0.5 h-4 w-4 shrink-0 ${item.textColor}`}
                      />
                      {req}
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald py-16">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <div className="mx-auto mb-5 flex h-12 w-12 items-center justify-center rounded-2xl bg-white/10">
            <Leaf className="h-6 w-6 text-green-300" />
          </div>
          <h2 className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl">
            See ESG scores in action
          </h2>
          <p className="mx-auto mt-3 max-w-2xl text-base leading-relaxed text-green-100/80">
            Browse our directory and compare supplier ESG scores side by side.
          </p>
          <div className="mt-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
            <Link
              href="/suppliers"
              className="inline-flex items-center gap-2 rounded-xl bg-white px-6 py-3 text-sm font-semibold text-brand-green-dark shadow-lg transition-all hover:bg-green-50 hover:shadow-xl"
            >
              Browse Suppliers
              <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              href="/get-listed"
              className="inline-flex items-center gap-2 rounded-xl border border-white/20 bg-white/10 px-6 py-3 text-sm font-semibold text-white backdrop-blur-sm transition-all hover:bg-white/20"
            >
              Get Listed
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
