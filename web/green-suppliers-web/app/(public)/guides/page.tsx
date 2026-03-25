import type { Metadata } from "next";
import Link from "next/link";
import { BookOpen, ArrowRight, Calendar } from "lucide-react";

// ---------------------------------------------------------------------------
// Metadata
// ---------------------------------------------------------------------------

export const metadata: Metadata = {
  title: "Sustainability Guides | Green Suppliers",
  description:
    "Expert guides on ESG scoring, green procurement, sustainability standards, and finding verified green suppliers in South Africa.",
};

// ---------------------------------------------------------------------------
// Static guide list (future: fetch from API)
// ---------------------------------------------------------------------------

interface GuidePreview {
  slug: string;
  title: string;
  description: string;
  publishedAt: string;
  category: string;
}

const guides: GuidePreview[] = [
  {
    slug: "what-is-esg-scoring",
    title: "What Is ESG Scoring? A Guide for South African Businesses",
    description:
      "Learn how ESG scoring works, why it matters for South African procurement, and how Green Suppliers verifies supplier sustainability.",
    publishedAt: "2026-01-15",
    category: "ESG",
  },
  {
    slug: "green-procurement-south-africa",
    title: "Green Procurement in South Africa: A Complete Guide",
    description:
      "Everything you need to know about sustainable procurement practices in South Africa. Standards, regulations, and how to find verified green suppliers.",
    publishedAt: "2026-02-10",
    category: "Procurement",
  },
  {
    slug: "iso-14001-certification-guide",
    title: "ISO 14001 Certification: What Suppliers Need to Know",
    description:
      "A step-by-step guide to achieving ISO 14001 environmental management certification and how it impacts your ESG score.",
    publishedAt: "2026-02-28",
    category: "Certification",
  },
  {
    slug: "renewable-energy-for-business",
    title: "Switching to Renewable Energy: A Business Guide",
    description:
      "How South African businesses can transition to renewable energy. Solar, wind, and battery storage options compared.",
    publishedAt: "2026-03-05",
    category: "Energy",
  },
  {
    slug: "carbon-reporting-for-smes",
    title: "Carbon Reporting for SMEs: Getting Started",
    description:
      "A practical guide for small and medium enterprises to begin carbon reporting and work towards net-zero targets.",
    publishedAt: "2026-03-18",
    category: "Carbon",
  },
];

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function formatDate(dateStr: string): string {
  try {
    return new Date(dateStr).toLocaleDateString("en-ZA", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  } catch {
    return dateStr;
  }
}

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

export default function GuidesIndexPage() {
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
            <span className="font-medium text-gray-900">Guides</span>
          </nav>
        </div>
      </div>

      {/* Header */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
          <div className="text-center">
            <div className="mx-auto mb-4 flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-green-light">
              <BookOpen className="h-6 w-6 text-brand-green" />
            </div>
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              Sustainability Guides
            </h1>
            <p className="mx-auto mt-3 max-w-2xl text-lg text-gray-500">
              Expert guides on ESG scoring, green procurement, certifications,
              and sustainable business practices in South Africa
            </p>
          </div>
        </div>
      </section>

      {/* Guides list */}
      <div className="mx-auto max-w-4xl px-4 py-10 sm:px-6 lg:px-8">
        <div className="flex flex-col gap-5">
          {guides.map((guide) => (
            <Link
              key={guide.slug}
              href={`/guides/${guide.slug}`}
              className="group flex flex-col gap-3 rounded-2xl border border-gray-100 bg-white p-6 shadow-sm transition-all hover:border-brand-green/20 hover:shadow-md hover:-translate-y-0.5"
            >
              <div className="flex items-center gap-2">
                <span className="inline-flex items-center rounded-full bg-brand-green-light px-2.5 py-0.5 text-xs font-semibold text-brand-green-dark">
                  {guide.category}
                </span>
                <span className="flex items-center gap-1 text-xs text-gray-400">
                  <Calendar className="h-3.5 w-3.5" />
                  {formatDate(guide.publishedAt)}
                </span>
              </div>

              <h2 className="text-lg font-semibold text-gray-900 group-hover:text-brand-green">
                {guide.title}
              </h2>

              <p className="text-sm leading-relaxed text-gray-500">
                {guide.description}
              </p>

              <span className="flex items-center text-sm font-medium text-brand-green group-hover:text-brand-green-hover">
                Read Guide
                <ArrowRight className="ml-1 h-4 w-4 transition-transform group-hover:translate-x-0.5" />
              </span>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
