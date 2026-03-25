import type { Metadata } from "next";
import Link from "next/link";
import { apiGet } from "@/lib/api-client";
import type { Industry } from "@/lib/types";
import { Factory, ArrowRight } from "lucide-react";

// ---------------------------------------------------------------------------
// Metadata
// ---------------------------------------------------------------------------

export const metadata: Metadata = {
  title: "Browse Green Industries | Green Suppliers Directory",
  description:
    "Explore verified green suppliers across all sustainable industries in South Africa. Renewable energy, waste management, green construction, and more.",
};

// ---------------------------------------------------------------------------
// Fallback data
// ---------------------------------------------------------------------------

const fallbackIndustries: Industry[] = [
  {
    id: "1",
    name: "Renewable Energy",
    slug: "renewable-energy",
    description:
      "Solar, wind, and other clean energy providers powering a sustainable future.",
    parentId: null,
    sortOrder: 1,
    isActive: true,
    supplierCount: 12,
  },
  {
    id: "2",
    name: "Waste Management",
    slug: "waste-management",
    description:
      "Recycling, composting, and waste reduction services for businesses and communities.",
    parentId: null,
    sortOrder: 2,
    isActive: true,
    supplierCount: 9,
  },
  {
    id: "3",
    name: "Sustainable Agriculture",
    slug: "sustainable-agriculture",
    description:
      "Organic farming, regenerative agriculture, and sustainable food production.",
    parentId: null,
    sortOrder: 3,
    isActive: true,
    supplierCount: 7,
  },
  {
    id: "4",
    name: "Green Construction",
    slug: "green-construction",
    description:
      "Eco-friendly building materials, green architecture, and sustainable construction practices.",
    parentId: null,
    sortOrder: 4,
    isActive: true,
    supplierCount: 8,
  },
  {
    id: "5",
    name: "Eco Packaging",
    slug: "eco-packaging",
    description:
      "Biodegradable, recyclable, and sustainable packaging solutions for all industries.",
    parentId: null,
    sortOrder: 5,
    isActive: true,
    supplierCount: 6,
  },
  {
    id: "6",
    name: "Water Solutions",
    slug: "water-solutions",
    description:
      "Water recycling, purification, and conservation technology providers.",
    parentId: null,
    sortOrder: 6,
    isActive: true,
    supplierCount: 5,
  },
  {
    id: "7",
    name: "Carbon Management",
    slug: "carbon-management",
    description:
      "Carbon offset programmes, emissions tracking, and net-zero consulting services.",
    parentId: null,
    sortOrder: 7,
    isActive: true,
    supplierCount: 4,
  },
  {
    id: "8",
    name: "Sustainable Transport",
    slug: "sustainable-transport",
    description:
      "Electric vehicles, green logistics, and low-emission transport solutions.",
    parentId: null,
    sortOrder: 8,
    isActive: true,
    supplierCount: 3,
  },
];

const industryEmojis: Record<string, string> = {
  "renewable-energy": "&#9728;&#65039;",
  "waste-management": "&#9851;&#65039;",
  "sustainable-agriculture": "&#127793;",
  "green-construction": "&#127959;&#65039;",
  "eco-packaging": "&#128230;",
  "water-solutions": "&#128167;",
  "carbon-management": "&#9729;&#65039;",
  "sustainable-transport": "&#128666;",
};

// ---------------------------------------------------------------------------
// Data fetching
// ---------------------------------------------------------------------------

async function getIndustries(): Promise<Industry[]> {
  try {
    const res = await apiGet<Industry[]>("/industries", {
      revalidate: 3600,
    });
    if (res.success && res.data && res.data.length > 0) {
      return res.data;
    }
  } catch {
    // API unreachable
  }
  return fallbackIndustries;
}

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

export default async function IndustriesIndexPage() {
  const industries = await getIndustries();

  const totalSuppliers = industries.reduce(
    (sum, ind) => sum + ind.supplierCount,
    0
  );

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
            <span className="font-medium text-gray-900">Industries</span>
          </nav>
        </div>
      </div>

      {/* Header */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
          <div className="text-center">
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              Browse by Industry
            </h1>
            <p className="mx-auto mt-3 max-w-2xl text-lg text-gray-500">
              Find verified green suppliers across {industries.length}{" "}
              sustainable industries with {totalSuppliers}+ listed suppliers
            </p>
          </div>
        </div>
      </section>

      {/* Industries grid */}
      <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          {industries.map((industry) => (
            <Link
              key={industry.id}
              href={`/industries/${industry.slug}`}
              className="group flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition-all hover:border-brand-green/20 hover:shadow-md hover:-translate-y-0.5"
            >
              {/* Green accent bar */}
              <div className="h-1.5 bg-gradient-to-r from-brand-green to-brand-emerald" />

              <div className="flex flex-col gap-3 p-6">
                <div className="flex items-start justify-between">
                  <span
                    className="text-3xl"
                    dangerouslySetInnerHTML={{
                      __html:
                        industryEmojis[industry.slug] ?? "&#127981;",
                    }}
                  />
                  <span className="inline-flex items-center rounded-full bg-brand-green-light px-2.5 py-0.5 text-xs font-semibold text-brand-green-dark">
                    {industry.supplierCount} supplier
                    {industry.supplierCount !== 1 ? "s" : ""}
                  </span>
                </div>

                <h2 className="text-lg font-semibold text-gray-900 group-hover:text-brand-green">
                  {industry.name}
                </h2>

                {industry.description && (
                  <p className="line-clamp-2 text-sm leading-relaxed text-gray-500">
                    {industry.description}
                  </p>
                )}

                <div className="mt-auto flex items-center text-sm font-medium text-brand-green group-hover:text-brand-green-hover">
                  View Suppliers
                  <ArrowRight className="ml-1 h-4 w-4 transition-transform group-hover:translate-x-0.5" />
                </div>
              </div>
            </Link>
          ))}
        </div>
      </div>
    </div>
  );
}
