import type { Metadata } from "next";
import { apiGet } from "@/lib/api-client";
import type { SupplierSearchResult } from "@/lib/types";
import { FilterSidebar } from "@/components/search/filter-sidebar";
import { SearchResults } from "@/components/search/search-results";
import { Search } from "lucide-react";
import { Suspense } from "react";

// ---------------------------------------------------------------------------
// Metadata
// ---------------------------------------------------------------------------

export const metadata: Metadata = {
  title: "Find Green Suppliers | Green Suppliers Directory",
  description:
    "Search and compare verified green suppliers across South Africa. Filter by industry, country, ESG level, and certification.",
};

// ---------------------------------------------------------------------------
// Fallback data (used when API is unreachable)
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
  {
    id: "3",
    slug: "juwi-south-africa",
    tradingName: "JUWI South Africa",
    shortDescription:
      "Global renewable energy leader constructing 340 MW of solar plants in South Africa.",
    city: "Cape Town",
    countryCode: "ZA",
    verificationStatus: "Unverified",
    esgLevel: "Bronze",
    esgScore: 25,
    logoUrl: null,
    industries: ["Renewable Energy"],
    isVerified: false,
  },
  {
    id: "4",
    slug: "solid-green",
    tradingName: "Solid Green",
    shortDescription:
      "South Africa's leading green building consultancy with 100+ Green Star SA certified projects.",
    city: "Johannesburg",
    countryCode: "ZA",
    verificationStatus: "Unverified",
    esgLevel: "Bronze",
    esgScore: 25,
    logoUrl: null,
    industries: ["Construction"],
    isVerified: false,
  },
  {
    id: "5",
    slug: "ecopack",
    tradingName: "EcoPack",
    shortDescription:
      "Premier manufacturer of 100% biodegradable and compostable food packaging from sugar cane and plant starches.",
    city: "Cape Town",
    countryCode: "ZA",
    verificationStatus: "Unverified",
    esgLevel: "Bronze",
    esgScore: 25,
    logoUrl: null,
    industries: ["Manufacturing", "Waste Management"],
    isVerified: false,
  },
  {
    id: "6",
    slug: "greencape",
    tradingName: "GreenCape",
    shortDescription:
      "Non-profit driving green economy adoption in South Africa through market intelligence and investment facilitation.",
    city: "Cape Town",
    countryCode: "ZA",
    verificationStatus: "Unverified",
    esgLevel: "Bronze",
    esgScore: 25,
    logoUrl: null,
    industries: ["Renewable Energy", "Waste Management", "Water Solutions"],
    isVerified: false,
  },
];

// ---------------------------------------------------------------------------
// Data fetching
// ---------------------------------------------------------------------------

interface SearchResponse {
  items: SupplierSearchResult[];
}

interface SearchParams {
  q?: string;
  countryCode?: string;
  industrySlug?: string;
  esgLevel?: string;
  verificationStatus?: string;
  page?: string;
}

async function searchSuppliers(params: SearchParams) {
  const query = new URLSearchParams();
  if (params.q) query.set("q", params.q);
  if (params.countryCode) query.set("countryCode", params.countryCode);
  if (params.industrySlug) query.set("industrySlug", params.industrySlug);
  if (params.esgLevel) query.set("esgLevel", params.esgLevel);
  if (params.verificationStatus)
    query.set("verificationStatus", params.verificationStatus);
  const page = parseInt(params.page ?? "1", 10);
  query.set("page", String(Math.max(1, page)));
  query.set("pageSize", "12");

  try {
    const res = await apiGet<SearchResponse>(
      `/suppliers?${query.toString()}`,
      { revalidate: 60 }
    );
    if (res.success && res.data?.items) {
      return {
        suppliers: res.data.items,
        total: res.meta?.total ?? res.data.items.length,
        page: res.meta?.page ?? page,
        totalPages: res.meta?.totalPages ?? 1,
      };
    }
  } catch {
    // API unreachable
  }

  // Fallback: apply basic client-side filtering on static data
  let filtered = [...fallbackSuppliers];
  if (params.q) {
    const lq = params.q.toLowerCase();
    filtered = filtered.filter(
      (s) =>
        s.tradingName.toLowerCase().includes(lq) ||
        s.shortDescription?.toLowerCase().includes(lq) ||
        s.industries.some((i) => i.toLowerCase().includes(lq))
    );
  }
  if (params.countryCode) {
    filtered = filtered.filter((s) => s.countryCode === params.countryCode);
  }
  if (params.esgLevel) {
    const levels = params.esgLevel.split(",");
    filtered = filtered.filter((s) => levels.includes(s.esgLevel));
  }
  if (params.verificationStatus === "Verified") {
    filtered = filtered.filter((s) => s.isVerified);
  }

  return {
    suppliers: filtered,
    total: filtered.length,
    page: 1,
    totalPages: 1,
  };
}

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

interface PageProps {
  searchParams: Promise<SearchParams>;
}

export default async function SuppliersSearchPage({ searchParams }: PageProps) {
  const params = await searchParams;
  const { suppliers, total, page, totalPages } = await searchSuppliers(params);

  return (
    <div className="min-h-screen bg-gray-50/50">
      {/* Page header */}
      <div className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
          <div className="flex flex-col gap-1">
            <h1 className="text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
              {params.q ? (
                <>
                  Results for &ldquo;{params.q}&rdquo;
                </>
              ) : (
                "Browse Green Suppliers"
              )}
            </h1>
            <p className="text-sm text-gray-500">
              Discover verified, ESG-compliant suppliers across South Africa and beyond
            </p>
          </div>

          {/* Search input (quick re-search) */}
          <form
            action="/suppliers"
            method="GET"
            className="mt-5 flex max-w-lg gap-2"
          >
            {/* Preserve existing filters */}
            {params.countryCode && (
              <input type="hidden" name="countryCode" value={params.countryCode} />
            )}
            {params.industrySlug && (
              <input type="hidden" name="industrySlug" value={params.industrySlug} />
            )}
            {params.esgLevel && (
              <input type="hidden" name="esgLevel" value={params.esgLevel} />
            )}
            {params.verificationStatus && (
              <input
                type="hidden"
                name="verificationStatus"
                value={params.verificationStatus}
              />
            )}
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
              <input
                type="text"
                name="q"
                defaultValue={params.q ?? ""}
                placeholder="Search suppliers, services, certifications..."
                className="h-10 w-full rounded-xl border border-gray-200 bg-white pl-9 pr-4 text-sm text-gray-700 outline-none transition-colors focus:border-brand-green focus:ring-2 focus:ring-brand-green/20"
              />
            </div>
            <button
              type="submit"
              className="h-10 rounded-xl bg-brand-green px-5 text-sm font-semibold text-white transition-colors hover:bg-brand-green-hover"
            >
              Search
            </button>
          </form>
        </div>
      </div>

      {/* Main content: sidebar + results */}
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="flex gap-8">
          {/* Filter sidebar */}
          <Suspense fallback={null}>
            <FilterSidebar />
          </Suspense>

          {/* Results */}
          <div className="min-w-0 flex-1">
            <SearchResults
              suppliers={suppliers}
              total={total}
              page={page}
              totalPages={totalPages}
              query={params.q}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
