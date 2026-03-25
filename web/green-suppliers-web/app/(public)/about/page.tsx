import type { Metadata } from "next";
import Link from "next/link";
import {
  Leaf,
  Shield,
  Globe,
  Users,
  BarChart3,
  ArrowRight,
  Target,
  Eye,
} from "lucide-react";

export const metadata: Metadata = {
  title: "About Us | Green Suppliers",
  description:
    "Green Suppliers is South Africa's trusted directory for verified green and sustainable suppliers. Learn about our mission, ESG scoring methodology, and the team behind the platform.",
};

const values = [
  {
    icon: Shield,
    title: "Transparency",
    description:
      "Every supplier is scored using a rules-based ESG system. No pay-to-play, no hidden criteria.",
  },
  {
    icon: BarChart3,
    title: "Data-Driven",
    description:
      "Our verification engine checks certifications, renewable energy usage, waste recycling, and carbon reporting.",
  },
  {
    icon: Globe,
    title: "Pan-African",
    description:
      "We started in South Africa and are expanding across the continent. Green procurement is a global imperative.",
  },
  {
    icon: Users,
    title: "Community",
    description:
      "We believe sustainable supply chains are built on trust between buyers and suppliers. We facilitate that trust.",
  },
];

export default function AboutPage() {
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
            <span className="font-medium text-gray-900">About</span>
          </nav>
        </div>
      </div>

      {/* Hero */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-12 sm:px-6 sm:py-16 lg:px-8">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mx-auto mb-5 flex h-14 w-14 items-center justify-center rounded-2xl bg-brand-green-light">
              <Leaf className="h-7 w-7 text-brand-green" />
            </div>
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              About Green Suppliers
            </h1>
            <p className="mx-auto mt-4 max-w-2xl text-lg leading-relaxed text-gray-500">
              We are building South Africa&apos;s most trusted directory of
              verified green and sustainable suppliers. Our mission is to make
              sustainable procurement simple, transparent, and accessible for
              every business in Africa.
            </p>
          </div>
        </div>
      </section>

      {/* Mission */}
      <section className="py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mx-auto max-w-3xl">
            <h2 className="text-2xl font-extrabold tracking-tight text-gray-900">
              Our Mission
            </h2>
            <div className="mt-4 space-y-4 text-base leading-relaxed text-gray-600">
              <p>
                Finding verified, ESG-compliant suppliers in Africa is hard.
                Procurement teams spend weeks on manual due diligence, and
                there is no centralised, trusted directory to simplify the
                process.
              </p>
              <p>
                Green Suppliers changes that. We provide a free, searchable
                directory where every supplier is independently assessed using a
                transparent, rules-based ESG scoring system. Buyers can filter
                by industry, country, certification, and ESG level to find
                exactly the right partner.
              </p>
              <p>
                For suppliers, we offer visibility, credibility, and
                connection to enterprise buyers who value sustainability. Our
                platform tracks certifications, monitors expiry dates, and
                automatically recalculates ESG scores.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Values */}
      <section className="bg-white py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
              Our Values
            </h2>
          </div>
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {values.map((value) => {
              const Icon = value.icon;
              return (
                <div
                  key={value.title}
                  className="flex flex-col items-center gap-3 rounded-2xl border border-gray-100 bg-gray-50/50 p-6 text-center"
                >
                  <div className="flex h-11 w-11 items-center justify-center rounded-2xl bg-brand-green-light">
                    <Icon className="h-5 w-5 text-brand-green" />
                  </div>
                  <h3 className="text-sm font-semibold text-gray-900">
                    {value.title}
                  </h3>
                  <p className="text-sm leading-relaxed text-gray-500">
                    {value.description}
                  </p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* SDG Alignment */}
      <section className="py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mx-auto max-w-3xl text-center">
            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">
              <Target className="h-3.5 w-3.5" />
              Global Framework
            </div>
            <h2 className="text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
              Aligned with the UN SDGs
            </h2>
            <p className="mx-auto mt-4 max-w-2xl text-base leading-relaxed text-gray-500">
              Our ESG scoring framework maps directly to 7 UN Sustainable
              Development Goals, giving buyers confidence that every verified
              supplier contributes to global sustainability targets.
            </p>
            <Link
              href="/verification"
              className="mt-6 inline-flex items-center gap-2 text-sm font-semibold text-brand-green transition-colors hover:text-brand-green-hover"
            >
              Learn about our verification methodology
              <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* Powered by Agilus */}
      <section className="border-t border-gray-100 bg-white py-16">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="mx-auto max-w-3xl text-center">
            <h2 className="text-2xl font-extrabold tracking-tight text-gray-900">
              Powered by Agilus.AI
            </h2>
            <p className="mx-auto mt-4 max-w-2xl text-base leading-relaxed text-gray-500">
              Green Suppliers is built and operated by{" "}
              <a
                href="https://agilus.ai"
                target="_blank"
                rel="noopener noreferrer"
                className="font-medium text-brand-green hover:underline"
              >
                Agilus.AI
              </a>
              , a South African technology company specialising in AI-powered
              enterprise software for procurement, supplier intelligence, and
              business automation.
            </p>
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald py-16">
        <div className="mx-auto max-w-7xl px-4 text-center sm:px-6 lg:px-8">
          <h2 className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl">
            Ready to get started?
          </h2>
          <p className="mx-auto mt-3 max-w-2xl text-base leading-relaxed text-green-100/80">
            Whether you are a buyer looking for verified green suppliers or a
            supplier wanting to showcase your ESG credentials, we have you
            covered.
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
