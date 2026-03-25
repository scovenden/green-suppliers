import type { Metadata } from "next";
import { notFound } from "next/navigation";
import Link from "next/link";
import { apiGet } from "@/lib/api-client";
import type { Industry, SupplierSearchResult } from "@/lib/types";
import { SupplierCard } from "@/components/suppliers/supplier-card";
import { ArrowLeft, ArrowRight, Leaf, Factory } from "lucide-react";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface IndustryDetailResponse {
  industry: Industry;
  suppliers: SupplierSearchResult[];
  meta: {
    page: number;
    pageSize: number;
    total: number;
    totalPages: number;
  };
}

interface PageProps {
  params: Promise<{ slug: string }>;
}

// ---------------------------------------------------------------------------
// Fallback data
// ---------------------------------------------------------------------------

const fallbackSuppliers: SupplierSearchResult[] = [
  {
    id: "1",
    slug: "artsolar",
    tradingName: "ARTsolar",
    shortDescription:
      "South Africa's only locally owned solar panel manufacturer, producing world-class PV modules in Durban since 2010.",
    city: "Durban",
    countryCode: "ZA",
    verificationStatus: "Unverified",
    esgLevel: "Bronze",
    esgScore: 25,
    logoUrl: null,
    industries: ["Renewable Energy", "Manufacturing"],
    isVerified: false,
  },
  {
    id: "2",
    slug: "mpact-group",
    tradingName: "Mpact Group",
    shortDescription:
      "Southern Africa's largest paper and plastics packaging and recycling business, collecting 588,000+ tonnes of recyclables annually.",
    city: "Johannesburg",
    countryCode: "ZA",
    verificationStatus: "Unverified",
    esgLevel: "Bronze",
    esgScore: 25,
    logoUrl: null,
    industries: ["Waste Management", "Manufacturing"],
    isVerified: false,
  },
];

// ---------------------------------------------------------------------------
// Data fetching
// ---------------------------------------------------------------------------

async function getIndustryDetail(
  slug: string
): Promise<IndustryDetailResponse | null> {
  try {
    const res = await apiGet<IndustryDetailResponse>(
      `/industries/${slug}`,
      { revalidate: 120 }
    );
    if (res.success && res.data) {
      return res.data;
    }
  } catch {
    // API unreachable
  }
  return null;
}

// ---------------------------------------------------------------------------
// Metadata
// ---------------------------------------------------------------------------

export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const data = await getIndustryDetail(slug);

  if (!data) {
    return { title: "Industry Not Found | Green Suppliers" };
  }

  const title = `${data.industry.name} Green Suppliers | Green Suppliers Directory`;
  const description =
    data.industry.description ??
    `Find verified green suppliers in the ${data.industry.name} industry. Compare ESG scores, certifications, and sustainability practices.`;

  return {
    title,
    description,
    openGraph: {
      title,
      description,
      siteName: "Green Suppliers",
    },
  };
}

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

export default async function IndustryPage({ params }: PageProps) {
  const { slug } = await params;
  const data = await getIndustryDetail(slug);

  if (!data) {
    notFound();
  }

  const { industry, suppliers, meta } = data;

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
            <Link
              href="/industries"
              className="transition-colors hover:text-brand-green"
            >
              Industries
            </Link>
            <span>/</span>
            <span className="font-medium text-gray-900">{industry.name}</span>
          </nav>
        </div>
      </div>

      {/* Hero section */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
          <div className="flex flex-col gap-4">
            <div className="flex items-center gap-3">
              <div className="flex h-12 w-12 items-center justify-center rounded-2xl bg-brand-green-light">
                <Factory className="h-6 w-6 text-brand-green" />
              </div>
              <h1 className="text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
                {industry.name}
              </h1>
            </div>
            {industry.description && (
              <p className="max-w-3xl text-base leading-relaxed text-gray-600">
                {industry.description}
              </p>
            )}
            <p className="text-sm text-gray-500">
              <span className="font-semibold text-gray-900">
                {meta.total}
              </span>{" "}
              verified supplier{meta.total !== 1 ? "s" : ""} in this industry
            </p>
          </div>
        </div>
      </section>

      {/* Supplier grid */}
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        {suppliers.length > 0 ? (
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {suppliers.map((supplier) => (
              <SupplierCard key={supplier.id} supplier={supplier} />
            ))}
          </div>
        ) : (
          <div className="flex flex-col items-center justify-center py-20 text-center">
            <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
              <Factory className="h-8 w-8 text-gray-400" />
            </div>
            <h3 className="text-lg font-semibold text-gray-900">
              No suppliers yet
            </h3>
            <p className="mt-2 max-w-md text-sm text-gray-500">
              We are actively onboarding suppliers in this industry. Be the
              first to get listed!
            </p>
          </div>
        )}

        {/* View all link */}
        {meta.total > meta.pageSize && (
          <div className="mt-10 text-center">
            <Link
              href={`/suppliers?industrySlug=${slug}`}
              className="inline-flex items-center gap-2 rounded-xl bg-brand-green-light px-6 py-3 text-sm font-semibold text-brand-green-dark transition-colors hover:bg-green-100"
            >
              View All {meta.total} Suppliers
              <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        )}
      </div>

      {/* CTA section */}
      <section className="bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald py-16">
        <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="pointer-events-none absolute inset-0 overflow-hidden">
            <div className="absolute -top-16 -right-16 h-64 w-64 rounded-full bg-white/5" />
            <div className="absolute -bottom-20 -left-20 h-80 w-80 rounded-full bg-white/5" />
          </div>
          <div className="relative text-center">
            <div className="mx-auto mb-5 flex h-12 w-12 items-center justify-center rounded-2xl bg-white/10">
              <Leaf className="h-6 w-6 text-green-300" />
            </div>
            <h2
              className="text-2xl font-extrabold tracking-tight text-white sm:text-3xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              Are you a green {industry.name.toLowerCase()} supplier?
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-base leading-relaxed text-green-100/80">
              Get listed in South Africa&apos;s fastest-growing green supplier
              directory. Showcase your ESG credentials and connect with
              enterprise buyers.
            </p>
            <div className="mt-6 flex flex-col items-center justify-center gap-4 sm:flex-row">
              <Link
                href="/get-listed"
                className="inline-flex items-center gap-2 rounded-xl bg-white px-6 py-3 text-sm font-semibold text-brand-green-dark shadow-lg transition-all hover:bg-green-50 hover:shadow-xl"
              >
                Get Listed for Free
                <ArrowRight className="h-4 w-4" />
              </Link>
              <Link
                href="/suppliers"
                className="inline-flex items-center gap-2 rounded-xl border border-white/20 bg-white/10 px-6 py-3 text-sm font-semibold text-white backdrop-blur-sm transition-all hover:bg-white/20"
              >
                Browse All Suppliers
              </Link>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
