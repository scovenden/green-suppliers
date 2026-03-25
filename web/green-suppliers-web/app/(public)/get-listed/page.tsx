import type { Metadata } from "next";
import Link from "next/link";
import { GetListedForm } from "@/components/leads/get-listed-form";
import {
  Shield,
  Award,
  TrendingUp,
  Users,
  CheckCircle,
  Leaf,
} from "lucide-react";

// ---------------------------------------------------------------------------
// Metadata
// ---------------------------------------------------------------------------

export const metadata: Metadata = {
  title: "Get Listed | Green Suppliers Directory",
  description:
    "Apply to be listed on South Africa's trusted green supplier directory. Showcase your ESG credentials, earn verification badges, and connect with enterprise buyers.",
};

// ---------------------------------------------------------------------------
// Trust signals data
// ---------------------------------------------------------------------------

const benefits = [
  {
    icon: <Shield className="h-5 w-5 text-brand-green" />,
    title: "Verified ESG Badge",
    description:
      "Earn a transparent ESG score (Bronze to Platinum) that buyers trust.",
  },
  {
    icon: <Users className="h-5 w-5 text-brand-green" />,
    title: "Enterprise Buyer Access",
    description:
      "Connect directly with procurement managers and ESG officers at leading companies.",
  },
  {
    icon: <TrendingUp className="h-5 w-5 text-brand-green" />,
    title: "SEO Visibility",
    description:
      "Your profile is optimised for search engines, helping buyers find you online.",
  },
  {
    icon: <Award className="h-5 w-5 text-brand-green" />,
    title: "Certification Tracking",
    description:
      "We track your certifications and remind you before they expire.",
  },
];

const steps = [
  {
    step: "1",
    title: "Submit Your Application",
    description:
      "Fill in the form below with your company details and sustainability information.",
  },
  {
    step: "2",
    title: "Review & Verification",
    description:
      "Our team reviews your submission and verifies your certifications and claims.",
  },
  {
    step: "3",
    title: "Go Live",
    description:
      "Your verified profile goes live in the directory with your ESG score and badge.",
  },
];

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

export default function GetListedPage() {
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
            <span className="font-medium text-gray-900">Get Listed</span>
          </nav>
        </div>
      </div>

      {/* Hero section */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mx-auto mb-5 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-green-light">
              <Leaf className="h-7 w-7 text-brand-green" />
            </div>
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              Get Listed on Green Suppliers
            </h1>
            <p className="mx-auto mt-3 max-w-2xl text-lg leading-relaxed text-gray-500">
              Join South Africa&apos;s fastest-growing directory of verified
              green suppliers. Showcase your sustainability credentials and
              connect with enterprise buyers.
            </p>
          </div>
        </div>
      </section>

      {/* Benefits */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {benefits.map((benefit) => (
              <div
                key={benefit.title}
                className="flex flex-col gap-2 rounded-2xl border border-gray-100 bg-gray-50/50 p-5"
              >
                <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-brand-green-light">
                  {benefit.icon}
                </div>
                <h3 className="text-sm font-semibold text-gray-900">
                  {benefit.title}
                </h3>
                <p className="text-sm leading-relaxed text-gray-500">
                  {benefit.description}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Main content: steps + form */}
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <div className="grid gap-10 lg:grid-cols-[300px_1fr]">
          {/* Left: How it works */}
          <div className="lg:sticky lg:top-24 lg:self-start">
            <h2 className="text-lg font-semibold text-gray-900">
              How It Works
            </h2>
            <div className="mt-4 flex flex-col gap-6">
              {steps.map((s) => (
                <div key={s.step} className="flex gap-3">
                  <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-brand-green text-sm font-bold text-white">
                    {s.step}
                  </div>
                  <div>
                    <h3 className="text-sm font-semibold text-gray-900">
                      {s.title}
                    </h3>
                    <p className="mt-0.5 text-sm leading-relaxed text-gray-500">
                      {s.description}
                    </p>
                  </div>
                </div>
              ))}
            </div>

            <div className="mt-8 rounded-2xl border border-green-200 bg-green-50 p-5">
              <h3 className="flex items-center gap-2 text-sm font-semibold text-brand-green-dark">
                <CheckCircle className="h-4 w-4" />
                Free to Get Listed
              </h3>
              <p className="mt-2 text-sm leading-relaxed text-gray-600">
                Basic listings are always free. Upgrade to Pro or Premium for
                enhanced visibility, lead analytics, and sponsored placements.
              </p>
            </div>
          </div>

          {/* Right: Form */}
          <div>
            <h2 className="mb-6 text-lg font-semibold text-gray-900">
              Your Application
            </h2>
            <GetListedForm />
          </div>
        </div>
      </div>
    </div>
  );
}
