import type { Metadata } from "next";
import Link from "next/link";
import {
  Shield,
  CheckCircle,
  ArrowRight,
  Leaf,
  FileCheck,
  BarChart3,
  Clock,
  AlertTriangle,
} from "lucide-react";

export const metadata: Metadata = {
  title: "How Verification Works | Green Suppliers",
  description:
    "Learn how Green Suppliers verifies supplier ESG credentials using a transparent, rules-based scoring system. Bronze, Silver, Gold, and Platinum tiers explained.",
};

const verificationTiers = [
  {
    level: "Bronze",
    description: "Basic profile complete with all required company fields filled.",
    gradient: "from-amber-700 to-amber-800",
    textColor: "text-amber-800",
    bgLight: "bg-amber-50",
    requirements: ["All required company fields filled"],
  },
  {
    level: "Silver",
    description:
      "At least 1 valid certification and 20%+ renewable energy usage.",
    gradient: "from-gray-400 to-gray-500",
    textColor: "text-gray-600",
    bgLight: "bg-gray-50",
    requirements: ["1+ valid certification", "20%+ renewable energy"],
  },
  {
    level: "Gold",
    description:
      "Multiple certifications, 50%+ renewable energy, and carbon reporting.",
    gradient: "from-amber-500 to-amber-600",
    textColor: "text-amber-700",
    bgLight: "bg-amber-50",
    requirements: [
      "2+ valid certifications",
      "50%+ renewable energy",
      "Carbon reporting",
    ],
  },
  {
    level: "Platinum",
    description:
      "Industry-leading sustainability across all measured dimensions.",
    gradient: "from-lime-600 to-green-700",
    textColor: "text-green-700",
    bgLight: "bg-green-50",
    requirements: [
      "3+ valid certifications",
      "70%+ renewable energy",
      "70%+ waste recycling",
      "Carbon reporting",
    ],
  },
];

const verificationSteps = [
  {
    icon: FileCheck,
    title: "Submit Credentials",
    description:
      "Suppliers provide their company details, sustainability data, and upload certifications.",
  },
  {
    icon: Shield,
    title: "Automated Assessment",
    description:
      "Our rules-based engine evaluates certifications, renewable energy usage, waste recycling, and carbon reporting.",
  },
  {
    icon: BarChart3,
    title: "Score & Tier Assigned",
    description:
      "Each supplier receives an ESG score and tier (Bronze, Silver, Gold, or Platinum) based on verified data.",
  },
  {
    icon: Clock,
    title: "Continuous Monitoring",
    description:
      "Scores are recalculated nightly. Expiring certifications trigger re-assessment and email reminders.",
  },
];

const verificationStatuses = [
  {
    status: "Verified",
    icon: CheckCircle,
    color: "text-brand-green",
    bg: "bg-brand-green-light",
    description:
      "At least 1 accepted certification uploaded, not expired, and all required company fields complete.",
  },
  {
    status: "Pending",
    icon: Clock,
    color: "text-amber-600",
    bg: "bg-amber-50",
    description:
      "Certification uploaded and awaiting validation by our verification team.",
  },
  {
    status: "Unverified",
    icon: Shield,
    color: "text-gray-500",
    bg: "bg-gray-50",
    description:
      "Default state. No certifications uploaded or profile is incomplete.",
  },
  {
    status: "Flagged",
    icon: AlertTriangle,
    color: "text-red-600",
    bg: "bg-red-50",
    description:
      "Suspicious activity detected. Profile is hidden from search until admin reviews and resolves the issue.",
  },
];

export default function VerificationPage() {
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
            <span className="font-medium text-gray-900">
              How Verification Works
            </span>
          </nav>
        </div>
      </div>

      {/* Hero */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 sm:py-16 lg:px-8">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mx-auto mb-5 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-green-light">
              <Shield className="h-7 w-7 text-brand-green" />
            </div>
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              How Verification Works
            </h1>
            <p className="mx-auto mt-4 max-w-2xl text-lg leading-relaxed text-gray-500">
              Our transparent, rules-based ESG scoring system ensures every
              supplier is assessed fairly. No pay-to-play. No hidden criteria.
            </p>
          </div>
        </div>
      </section>

      {/* Process Steps */}
      <section className="py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-center text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
            The Verification Process
          </h2>
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {verificationSteps.map((step, index) => {
              const Icon = step.icon;
              return (
                <div
                  key={step.title}
                  className="flex flex-col items-center gap-3 rounded-2xl border border-gray-100 bg-white p-6 text-center shadow-sm"
                >
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-brand-green text-sm font-bold text-white">
                    {index + 1}
                  </div>
                  <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-brand-green-light">
                    <Icon className="h-5 w-5 text-brand-green" />
                  </div>
                  <h3 className="text-sm font-semibold text-gray-900">
                    {step.title}
                  </h3>
                  <p className="text-sm leading-relaxed text-gray-500">
                    {step.description}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* ESG Tiers */}
      <section className="bg-white py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
              ESG Scoring Tiers
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-base text-gray-500">
              Suppliers are assessed and placed into one of four tiers based on
              their sustainability credentials
            </p>
          </div>
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {verificationTiers.map((tier) => (
              <div
                key={tier.level}
                className="flex flex-col overflow-hidden rounded-3xl border border-gray-100 bg-white shadow-sm"
              >
                <div
                  className={`bg-gradient-to-r ${tier.gradient} px-5 py-4`}
                >
                  <h3 className="text-lg font-bold text-white">
                    {tier.level}
                  </h3>
                </div>
                <div className="flex flex-1 flex-col gap-3 p-5">
                  <p className="text-sm leading-relaxed text-gray-600">
                    {tier.description}
                  </p>
                  <ul className="mt-auto space-y-2">
                    {tier.requirements.map((req) => (
                      <li
                        key={req}
                        className="flex items-start gap-2 text-sm text-gray-600"
                      >
                        <CheckCircle
                          className={`mt-0.5 h-4 w-4 shrink-0 ${tier.textColor}`}
                        />
                        {req}
                      </li>
                    ))}
                  </ul>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Verification Statuses */}
      <section className="py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <h2 className="text-center text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
            Verification Statuses
          </h2>
          <div className="mt-10 grid gap-6 sm:grid-cols-2">
            {verificationStatuses.map((item) => {
              const Icon = item.icon;
              return (
                <div
                  key={item.status}
                  className="flex items-start gap-4 rounded-2xl border border-gray-100 bg-white p-6 shadow-sm"
                >
                  <div
                    className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-xl ${item.bg}`}
                  >
                    <Icon className={`h-5 w-5 ${item.color}`} />
                  </div>
                  <div>
                    <h3 className="text-sm font-semibold text-gray-900">
                      {item.status}
                    </h3>
                    <p className="mt-1 text-sm leading-relaxed text-gray-500">
                      {item.description}
                    </p>
                  </div>
                </div>
              );
            })}
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
            Ready to get verified?
          </h2>
          <p className="mx-auto mt-3 max-w-2xl text-base leading-relaxed text-green-100/80">
            Join South Africa&apos;s most trusted green supplier directory. Free
            to get listed.
          </p>
          <div className="mt-8 flex flex-col items-center justify-center gap-4 sm:flex-row">
            <Link
              href="/get-listed"
              className="inline-flex items-center gap-2 rounded-xl bg-white px-6 py-3 text-sm font-semibold text-brand-green-dark shadow-lg transition-all hover:bg-green-50 hover:shadow-xl"
            >
              Get Listed for Free
              <ArrowRight className="h-4 w-4" />
            </Link>
            <Link
              href="/esg-scoring"
              className="inline-flex items-center gap-2 rounded-xl border border-white/20 bg-white/10 px-6 py-3 text-sm font-semibold text-white backdrop-blur-sm transition-all hover:bg-white/20"
            >
              Learn About ESG Scoring
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
