import Link from "next/link";
import { SearchHero } from "@/components/search/search-hero";
import { SupplierCard } from "@/components/suppliers/supplier-card";
import { apiGet } from "@/lib/api-client";
import type { SupplierSearchResult, Industry } from "@/lib/types";
import {
  ArrowRight,
  CheckCircle,
  Leaf,
  Shield,
  Award,
  TrendingUp,
  Users,
  Globe,
  Factory,
} from "lucide-react";

// ---------------------------------------------------------------------------
// Static fallback data (used when the API is unreachable)
// ---------------------------------------------------------------------------

const fallbackSuppliers: SupplierSearchResult[] = [
  {
    id: "1",
    slug: "solaris-energy-solutions",
    tradingName: "Solaris Energy Solutions",
    shortDescription:
      "Leading provider of commercial solar installations and energy storage systems across Southern Africa.",
    city: "Cape Town",
    countryCode: "ZA",
    verificationStatus: "Verified",
    esgLevel: "gold",
    esgScore: 78,
    logoUrl: null,
    industries: ["Renewable Energy", "Energy Storage"],
    isVerified: true,
  },
  {
    id: "2",
    slug: "greenpack-sa",
    tradingName: "GreenPack SA",
    shortDescription:
      "Sustainable packaging manufacturer using 100% recycled and biodegradable materials for FMCG and retail.",
    city: "Johannesburg",
    countryCode: "ZA",
    verificationStatus: "Verified",
    esgLevel: "platinum",
    esgScore: 92,
    logoUrl: null,
    industries: ["Sustainable Packaging", "Manufacturing"],
    isVerified: true,
  },
  {
    id: "3",
    slug: "aqua-cycle-water",
    tradingName: "Aqua Cycle Water",
    shortDescription:
      "Industrial water recycling and treatment solutions. Helping mines and factories reduce freshwater consumption.",
    city: "Durban",
    countryCode: "ZA",
    verificationStatus: "Verified",
    esgLevel: "silver",
    esgScore: 61,
    logoUrl: null,
    industries: ["Water Solutions", "Waste Management"],
    isVerified: true,
  },
];

const fallbackIndustries = [
  { name: "Renewable Energy", slug: "renewable-energy", icon: "sun", count: 12 },
  { name: "Waste Management", slug: "waste-management", icon: "recycle", count: 9 },
  { name: "Sustainable Agriculture", slug: "sustainable-agriculture", icon: "sprout", count: 7 },
  { name: "Green Construction", slug: "green-construction", icon: "building", count: 8 },
  { name: "Eco Packaging", slug: "eco-packaging", icon: "package", count: 6 },
  { name: "Water Solutions", slug: "water-solutions", icon: "droplets", count: 5 },
  { name: "Carbon Management", slug: "carbon-management", icon: "cloud", count: 4 },
  { name: "Sustainable Transport", slug: "sustainable-transport", icon: "truck", count: 3 },
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

const industryIconMap: Record<string, string> = {
  "renewable-energy": "SUN",
  "waste-management": "RECYCLE",
  "sustainable-agriculture": "SPROUT",
  "green-construction": "BUILDING",
  "eco-packaging": "PACKAGE",
  "water-solutions": "DROPLETS",
  "carbon-management": "CLOUD",
  "sustainable-transport": "TRUCK",
};

// ---------------------------------------------------------------------------
// Data fetching
// ---------------------------------------------------------------------------

async function getFeaturedSuppliers(): Promise<SupplierSearchResult[]> {
  try {
    const res = await apiGet<{ items: SupplierSearchResult[] }>(
      "/suppliers?pageSize=3&sortBy=esgScore&sortDir=desc",
      { revalidate: 300 }
    );
    if (res.success && res.data?.items && res.data.items.length > 0) {
      return res.data.items;
    }
  } catch {
    // API unreachable — fall back to static data
  }
  return fallbackSuppliers;
}

async function getIndustries(): Promise<typeof fallbackIndustries> {
  try {
    const res = await apiGet<Industry[]>("/taxonomy/industries", {
      revalidate: 3600,
    });
    if (res.success && res.data && res.data.length > 0) {
      return res.data.map((ind) => ({
        name: ind.name,
        slug: ind.slug,
        icon: industryIconMap[ind.slug] ?? "FACTORY",
        count: ind.supplierCount,
      }));
    }
  } catch {
    // API unreachable
  }
  return fallbackIndustries;
}

// ---------------------------------------------------------------------------
// Verification tier data
// ---------------------------------------------------------------------------

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
    description: "At least 1 valid certification and 20%+ renewable energy usage.",
    gradient: "from-gray-400 to-gray-500",
    textColor: "text-gray-600",
    bgLight: "bg-gray-50",
    requirements: ["1+ valid certification", "20%+ renewable energy"],
  },
  {
    level: "Gold",
    description: "Multiple certifications, 50%+ renewable energy, and carbon reporting.",
    gradient: "from-amber-500 to-amber-600",
    textColor: "text-amber-700",
    bgLight: "bg-amber-50",
    requirements: ["2+ valid certifications", "50%+ renewable energy", "Carbon reporting"],
  },
  {
    level: "Platinum",
    description: "Industry-leading sustainability across all measured dimensions.",
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

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

export default async function HomePage() {
  const [suppliers, industries] = await Promise.all([
    getFeaturedSuppliers(),
    getIndustries(),
  ]);

  return (
    <div className="flex flex-col">
      {/* Hero section with search */}
      <SearchHero />

      {/* Stats bar — floating above the next section */}
      <div className="relative z-10 -mt-7">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-center gap-8 rounded-2xl bg-white px-8 py-5 shadow-lg sm:gap-12 md:gap-16">
            <div className="flex flex-col items-center gap-1">
              <div className="flex items-center gap-2">
                <Users className="h-5 w-5 text-brand-green" />
                <span className="text-2xl font-extrabold text-gray-900">50+</span>
              </div>
              <span className="text-sm text-gray-500">Suppliers</span>
            </div>
            <div className="h-8 w-px bg-gray-200" />
            <div className="flex flex-col items-center gap-1">
              <div className="flex items-center gap-2">
                <Factory className="h-5 w-5 text-brand-green" />
                <span className="text-2xl font-extrabold text-gray-900">8</span>
              </div>
              <span className="text-sm text-gray-500">Industries</span>
            </div>
            <div className="h-8 w-px bg-gray-200" />
            <div className="flex flex-col items-center gap-1">
              <div className="flex items-center gap-2">
                <Globe className="h-5 w-5 text-brand-green" />
                <span className="text-2xl font-extrabold text-gray-900">10</span>
              </div>
              <span className="text-sm text-gray-500">Countries</span>
            </div>
          </div>
        </div>
      </div>

      {/* Featured Suppliers */}
      <section className="py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900" style={{ letterSpacing: "-0.5px" }}>
              Featured Suppliers
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-lg text-gray-500">
              Discover top-rated green suppliers with the highest ESG scores
            </p>
          </div>

          <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {suppliers.map((supplier) => (
              <SupplierCard key={supplier.id} supplier={supplier} />
            ))}
          </div>

          <div className="mt-10 text-center">
            <Link
              href="/suppliers"
              className="inline-flex items-center gap-2 rounded-xl bg-brand-green-light px-6 py-3 text-sm font-semibold text-brand-green-dark transition-colors hover:bg-green-100"
            >
              View All Suppliers
              <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* Browse by Industry */}
      <section className="bg-gray-50 py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900" style={{ letterSpacing: "-0.5px" }}>
              Browse by Industry
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-lg text-gray-500">
              Find verified green suppliers in your sector
            </p>
          </div>

          <div className="mt-12 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
            {industries.map((industry) => (
              <Link
                key={industry.slug}
                href={`/industries/${industry.slug}`}
                className="group flex flex-col items-center gap-3 rounded-2xl border border-gray-100 bg-white p-6 text-center shadow-sm transition-all hover:border-brand-green/20 hover:shadow-md hover:-translate-y-0.5"
              >
                <span
                  className="text-3xl"
                  dangerouslySetInnerHTML={{
                    __html: industryEmojis[industry.slug] ?? "&#127981;",
                  }}
                />
                <h3 className="text-sm font-semibold text-gray-900 group-hover:text-brand-green">
                  {industry.name}
                </h3>
                <span className="text-xs text-gray-400">
                  {industry.count} supplier{industry.count !== 1 ? "s" : ""}
                </span>
              </Link>
            ))}
          </div>
        </div>
      </section>

      {/* How Verification Works */}
      <section className="py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900" style={{ letterSpacing: "-0.5px" }}>
              How Verification Works
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-lg text-gray-500">
              Our rules-based ESG scoring system ensures every supplier is
              assessed fairly and transparently
            </p>
          </div>

          <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {verificationTiers.map((tier) => (
              <div
                key={tier.level}
                className="flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm"
              >
                {/* Gradient header */}
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

      {/* CTA — Are You a Green Supplier? */}
      <section className="bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald py-20">
        <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          {/* Decorative circles */}
          <div className="pointer-events-none absolute inset-0 overflow-hidden">
            <div className="absolute -top-16 -right-16 h-64 w-64 rounded-full bg-white/5" />
            <div className="absolute -bottom-20 -left-20 h-80 w-80 rounded-full bg-white/5" />
          </div>

          <div className="relative text-center">
            <div className="mx-auto mb-6 flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10">
              <Leaf className="h-7 w-7 text-green-300" />
            </div>
            <h2
              className="text-3xl font-extrabold tracking-tight text-white sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              Are You a Green Supplier?
            </h2>
            <p className="mx-auto mt-4 max-w-2xl text-lg leading-relaxed text-green-100/80">
              Join South Africa&apos;s fastest-growing green supplier directory.
              Showcase your ESG credentials, earn verification badges, and
              connect with enterprise buyers.
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
                href="/verification"
                className="inline-flex items-center gap-2 rounded-xl border border-white/20 bg-white/10 px-6 py-3 text-sm font-semibold text-white backdrop-blur-sm transition-all hover:bg-white/20"
              >
                Learn About Verification
              </Link>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
