import Link from "next/link";
import Image from "next/image";
import { SearchHero } from "@/components/search/search-hero";
import { SupplierCard } from "@/components/suppliers/supplier-card";
import { TrustBadges } from "@/components/suppliers/trust-badges";
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
  BookOpen,
  FileCheck,
  BarChart3,
  Clock,
  Zap,
  Target,
  Eye,
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

const industryIconBg: Record<string, string> = {
  "renewable-energy": "bg-amber-50 text-amber-600",
  "waste-management": "bg-green-50 text-green-600",
  "sustainable-agriculture": "bg-lime-50 text-lime-600",
  "green-construction": "bg-slate-50 text-slate-600",
  "eco-packaging": "bg-teal-50 text-teal-600",
  "water-solutions": "bg-blue-50 text-blue-600",
  "carbon-management": "bg-gray-50 text-gray-600",
  "sustainable-transport": "bg-purple-50 text-purple-600",
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
    barColor: "bg-amber-600",
    barWidth: "25%",
    requirements: ["All required company fields filled"],
  },
  {
    level: "Silver",
    description: "At least 1 valid certification and 20%+ renewable energy usage.",
    gradient: "from-gray-400 to-gray-500",
    textColor: "text-gray-600",
    bgLight: "bg-gray-50",
    barColor: "bg-gray-400",
    barWidth: "50%",
    requirements: ["1+ valid certification", "20%+ renewable energy"],
  },
  {
    level: "Gold",
    description: "Multiple certifications, 50%+ renewable energy, and carbon reporting.",
    gradient: "from-amber-500 to-amber-600",
    textColor: "text-amber-700",
    bgLight: "bg-amber-50",
    barColor: "bg-amber-500",
    barWidth: "75%",
    requirements: ["2+ valid certifications", "50%+ renewable energy", "Carbon reporting"],
  },
  {
    level: "Platinum",
    description: "Industry-leading sustainability across all measured dimensions.",
    gradient: "from-lime-600 to-green-700",
    textColor: "text-green-700",
    bgLight: "bg-green-50",
    barColor: "bg-green-600",
    barWidth: "100%",
    requirements: [
      "3+ valid certifications",
      "70%+ renewable energy",
      "70%+ waste recycling",
      "Carbon reporting",
    ],
  },
];

// ---------------------------------------------------------------------------
// UN SDG data
// ---------------------------------------------------------------------------

const sdgGoals = [
  {
    number: 7,
    name: "Affordable and Clean Energy",
    color: "#FCC30B",
    description: "We connect buyers with renewable energy suppliers driving clean power adoption across Africa.",
  },
  {
    number: 9,
    name: "Industry, Innovation and Infrastructure",
    color: "#FD6925",
    description: "Promoting sustainable industrialisation and fostering innovation in green manufacturing.",
  },
  {
    number: 11,
    name: "Sustainable Cities and Communities",
    color: "#FD9D24",
    description: "Listing suppliers who build resilient infrastructure and deliver sustainable urban solutions.",
  },
  {
    number: 12,
    name: "Responsible Consumption and Production",
    color: "#BF8B2E",
    description: "Championing suppliers who minimise waste and adopt circular economy practices.",
  },
  {
    number: 13,
    name: "Climate Action",
    color: "#3F7E44",
    description: "Tracking carbon reporting and supporting suppliers committed to measurable climate targets.",
  },
  {
    number: 14,
    name: "Life Below Water",
    color: "#0A97D9",
    description: "Verifying water management practices and supporting suppliers who protect aquatic ecosystems.",
  },
  {
    number: 15,
    name: "Life on Land",
    color: "#56C02B",
    description: "Promoting sustainable agriculture, forestry, and biodiversity-conscious supply chains.",
  },
];

// ---------------------------------------------------------------------------
// Blog articles data
// ---------------------------------------------------------------------------

const blogArticles = [
  {
    id: "1",
    slug: "rise-of-green-procurement-south-africa",
    title: "The Rise of Green Procurement in South Africa",
    excerpt:
      "How South African enterprises are reshaping their supply chains with ESG-compliant sourcing strategies and what this means for the broader African market.",
    category: "Trends",
    categoryColor: "bg-emerald-100 text-emerald-700",
    readTime: "5 min read",
    gradientFrom: "from-emerald-500",
    gradientTo: "to-green-600",
    Icon: TrendingUp,
  },
  {
    id: "2",
    slug: "iso-14001-certification-guide-african-businesses",
    title: "ISO 14001 Certification: A Complete Guide for African Businesses",
    excerpt:
      "Everything you need to know about achieving ISO 14001 environmental management certification, from preparation to audit and beyond.",
    category: "Guides",
    categoryColor: "bg-amber-100 text-amber-700",
    readTime: "8 min read",
    gradientFrom: "from-amber-500",
    gradientTo: "to-orange-600",
    Icon: FileCheck,
  },
  {
    id: "3",
    slug: "esg-scoring-transforming-supplier-selection",
    title: "How ESG Scoring is Transforming Supplier Selection",
    excerpt:
      "Data-driven sustainability scoring is replacing gut-feel procurement decisions. Learn how transparent ESG metrics are levelling the playing field.",
    category: "Insights",
    categoryColor: "bg-blue-100 text-blue-700",
    readTime: "6 min read",
    gradientFrom: "from-blue-500",
    gradientTo: "to-indigo-600",
    Icon: BarChart3,
  },
];

// ---------------------------------------------------------------------------
// Trust strip partner badges
// ---------------------------------------------------------------------------

const trustPartners = [
  { name: "ISO 14001", abbr: "ISO" },
  { name: "B-Corp", abbr: "B" },
  { name: "FSC Certified", abbr: "FSC" },
  { name: "GBCSA", abbr: "GBCSA" },
  { name: "Carbon Trust", abbr: "CT" },
  { name: "Fair Trade", abbr: "FT" },
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
      {/* ================================================================= */}
      {/* 1. Hero section with search (existing SearchHero) */}
      {/* ================================================================= */}
      <SearchHero />

      {/* ================================================================= */}
      {/* 2. Stats bar — floating above the next section */}
      {/* ================================================================= */}
      <div className="relative z-10 -mt-7">
        <div className="mx-auto max-w-4xl px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-center gap-8 rounded-3xl bg-white px-8 py-6 shadow-organic sm:gap-12 md:gap-16">
            <div className="flex flex-col items-center gap-1">
              <div className="flex items-center gap-2">
                <Users className="h-5 w-5 text-brand-green" />
                <span className="animate-stat-pulse text-2xl font-extrabold text-gray-900">50+</span>
              </div>
              <span className="text-sm text-gray-500">Suppliers</span>
            </div>
            <div className="h-8 w-px bg-gray-200" />
            <div className="flex flex-col items-center gap-1">
              <div className="flex items-center gap-2">
                <Factory className="h-5 w-5 text-brand-green" />
                <span className="animate-stat-pulse text-2xl font-extrabold text-gray-900" style={{ animationDelay: "0.5s" }}>8</span>
              </div>
              <span className="text-sm text-gray-500">Industries</span>
            </div>
            <div className="h-8 w-px bg-gray-200" />
            <div className="flex flex-col items-center gap-1">
              <div className="flex items-center gap-2">
                <Globe className="h-5 w-5 text-brand-green" />
                <span className="animate-stat-pulse text-2xl font-extrabold text-gray-900" style={{ animationDelay: "1s" }}>10</span>
              </div>
              <span className="text-sm text-gray-500">Countries</span>
            </div>
          </div>
        </div>
      </div>

      {/* ================================================================= */}
      {/* 3. Trust Strip (NEW) */}
      {/* ================================================================= */}
      <section className="bg-brand-green-light/60 py-6">
        <div className="mx-auto max-w-6xl px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col items-center gap-4 sm:flex-row sm:justify-between">
            <p className="text-sm font-medium text-gray-600">
              Trusted by procurement teams across South Africa
            </p>
            <div className="flex flex-wrap items-center justify-center gap-3">
              {trustPartners.map((partner) => (
                <div
                  key={partner.name}
                  className="flex items-center gap-1.5 rounded-full border border-green-200/60 bg-white/80 px-3.5 py-1.5 text-xs font-semibold text-gray-600 shadow-sm backdrop-blur-sm"
                >
                  <span className="flex h-5 w-5 items-center justify-center rounded-full bg-brand-green/10 text-[9px] font-bold text-brand-green">
                    {partner.abbr.charAt(0)}
                  </span>
                  {partner.name}
                </div>
              ))}
            </div>
          </div>
        </div>
      </section>

      {/* Trust Badges section (existing) */}
      <TrustBadges />

      {/* ================================================================= */}
      {/* 4. Featured Suppliers (enhanced with background visual) */}
      {/* ================================================================= */}
      <section className="relative overflow-hidden py-20">
        {/* Background image overlay — solar panels */}
        <div className="pointer-events-none absolute inset-0">
          <Image
            src="https://images.unsplash.com/photo-1509391366360-2e959784a276?w=1920&q=80"
            alt=""
            fill
            className="object-cover opacity-[0.03]"
            sizes="100vw"
            priority={false}
          />
          <div className="absolute inset-0 bg-gradient-to-b from-white via-white/95 to-white" />
        </div>

        <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full bg-brand-green-light px-3 py-1 text-xs font-semibold text-brand-green">
              <Award className="h-3.5 w-3.5" />
              Top Rated
            </div>
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl">
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
              className="inline-flex items-center gap-2 rounded-2xl bg-brand-green-light px-6 py-3 text-sm font-semibold text-brand-green-dark transition-all duration-300 ease-out hover:bg-green-100 hover:shadow-organic"
            >
              View All Suppliers
              <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* ================================================================= */}
      {/* 5. UN Sustainable Development Goals (NEW - CRITICAL) */}
      {/* ================================================================= */}
      <section className="relative overflow-hidden bg-white py-20">
        {/* Subtle background pattern */}
        <div className="pointer-events-none absolute inset-0 opacity-[0.02]">
          <div className="absolute inset-0" style={{
            backgroundImage: "radial-gradient(circle at 1px 1px, #16A34A 1px, transparent 0)",
            backgroundSize: "40px 40px",
          }} />
        </div>

        <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full border border-blue-200 bg-blue-50 px-3 py-1 text-xs font-semibold text-blue-700">
              <Globe className="h-3.5 w-3.5" />
              Global Framework
            </div>
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl">
              Aligned with the UN Sustainable Development Goals
            </h2>
            <p className="mx-auto mt-4 max-w-3xl text-lg leading-relaxed text-gray-500">
              Every supplier on our platform is assessed against globally recognized sustainability
              frameworks, ensuring meaningful environmental and social impact.
            </p>
          </div>

          {/* SDG grid */}
          <div className="mt-14 grid gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {sdgGoals.map((sdg) => (
              <div
                key={sdg.number}
                className="group flex flex-col overflow-hidden rounded-2xl shadow-sm transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1"
              >
                {/* Colored header with large SDG number */}
                <div
                  className="flex items-center gap-3 px-5 py-4"
                  style={{ backgroundColor: sdg.color }}
                >
                  <span className="text-4xl font-bold text-white/90">
                    {sdg.number}
                  </span>
                  <h3 className="text-sm font-bold leading-tight text-white">
                    {sdg.name}
                  </h3>
                </div>

                {/* Description */}
                <div className="flex flex-1 flex-col bg-gray-50 p-4">
                  <p className="text-sm leading-relaxed text-gray-600">
                    {sdg.description}
                  </p>
                </div>
              </div>
            ))}

            {/* "Why SDGs" summary card */}
            <div className="flex flex-col justify-center rounded-2xl border-2 border-dashed border-gray-200 bg-gray-50/50 p-6 text-center">
              <Target className="mx-auto mb-3 h-8 w-8 text-brand-green" />
              <h3 className="text-sm font-bold text-gray-900">
                7 SDGs Tracked
              </h3>
              <p className="mt-2 text-xs leading-relaxed text-gray-500">
                Our ESG scoring framework maps directly to these UN goals, giving you confidence that every verified supplier contributes to global sustainability targets.
              </p>
            </div>
          </div>

          <div className="mt-10 text-center">
            <Link
              href="/verification"
              className="inline-flex items-center gap-2 text-sm font-semibold text-brand-green transition-colors hover:text-brand-green-hover"
            >
              Learn More about our SDG Commitment
              <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* ================================================================= */}
      {/* 6. How Verification Works (existing) */}
      {/* ================================================================= */}
      <section className="bg-gray-50/50 py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-700">
              <Shield className="h-3.5 w-3.5" />
              Transparent Methodology
            </div>
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl">
              How Verification Works
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-lg text-gray-500">
              Our rules-based ESG scoring system ensures every supplier is
              assessed fairly and transparently
            </p>
          </div>

          {/* Horizontal progress visualization */}
          <div className="mx-auto mt-12 max-w-3xl">
            <div className="relative h-3 w-full overflow-hidden rounded-full bg-gray-100">
              {verificationTiers.map((tier, i) => (
                <div
                  key={tier.level}
                  className={`absolute inset-y-0 left-0 rounded-full ${tier.barColor} transition-all duration-700 ease-out`}
                  style={{ width: tier.barWidth, zIndex: verificationTiers.length - i }}
                />
              ))}
            </div>
            <div className="mt-2 flex justify-between text-xs font-medium text-gray-500">
              {verificationTiers.map((tier) => (
                <span key={tier.level} className={tier.textColor}>{tier.level}</span>
              ))}
            </div>
          </div>

          {/* Tier cards */}
          <div className="mt-10 grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
            {verificationTiers.map((tier) => (
              <div
                key={tier.level}
                className="flex flex-col overflow-hidden rounded-3xl border border-gray-100 bg-white shadow-sm transition-all duration-300 ease-out hover:shadow-organic hover:-translate-y-1"
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

      {/* ================================================================= */}
      {/* 7. Latest from the Green Economy (NEW - Blog Section) */}
      {/* ================================================================= */}
      <section className="bg-[#FAFAFA] py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full bg-green-50 px-3 py-1 text-xs font-semibold text-brand-green">
              <BookOpen className="h-3.5 w-3.5" />
              Knowledge Hub
            </div>
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl">
              Latest from the Green Economy
            </h2>
            <p className="mx-auto mt-3 max-w-2xl text-lg text-gray-500">
              Insights, trends, and stories from Africa&apos;s sustainable business community
            </p>
          </div>

          <div className="mt-12 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {blogArticles.map((article) => {
              const ArticleIcon = article.Icon;
              return (
                <Link
                  key={article.id}
                  href={`/guides/${article.slug}`}
                  className="group flex flex-col overflow-hidden rounded-2xl bg-white shadow-sm transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1"
                >
                  {/* Image placeholder — gradient with icon */}
                  <div
                    className={`relative flex h-48 items-center justify-center bg-gradient-to-br ${article.gradientFrom} ${article.gradientTo}`}
                  >
                    <ArticleIcon className="h-16 w-16 text-white/20" />
                    {/* Category badge overlay */}
                    <span
                      className={`absolute left-4 top-4 rounded-full px-3 py-1 text-xs font-semibold ${article.categoryColor}`}
                    >
                      {article.category}
                    </span>
                  </div>

                  <div className="flex flex-1 flex-col gap-3 p-5">
                    <h3 className="text-base font-bold leading-snug text-gray-900 transition-colors group-hover:text-brand-green">
                      {article.title}
                    </h3>
                    <p className="flex-1 text-sm leading-relaxed text-gray-500">
                      {article.excerpt}
                    </p>
                    <div className="flex items-center justify-between border-t border-gray-100 pt-3">
                      <span className="flex items-center gap-1 text-xs text-gray-400">
                        <Clock className="h-3.5 w-3.5" />
                        {article.readTime}
                      </span>
                      <span className="flex items-center gap-1 text-xs font-semibold text-brand-green transition-colors group-hover:text-brand-green-hover">
                        Read More
                        <ArrowRight className="h-3.5 w-3.5 transition-transform group-hover:translate-x-0.5" />
                      </span>
                    </div>
                  </div>
                </Link>
              );
            })}
          </div>

          <div className="mt-10 text-center">
            <Link
              href="/guides"
              className="inline-flex items-center gap-2 text-sm font-semibold text-brand-green transition-colors hover:text-brand-green-hover"
            >
              View All Articles
              <ArrowRight className="h-4 w-4" />
            </Link>
          </div>
        </div>
      </section>

      {/* ================================================================= */}
      {/* 8. Impact By Numbers (NEW) */}
      {/* ================================================================= */}
      <section className="relative overflow-hidden">
        {/* Background image overlay — forest */}
        <div className="pointer-events-none absolute inset-0">
          <Image
            src="https://images.unsplash.com/photo-1441974231531-c6227db76b6e?w=1920&q=80"
            alt=""
            fill
            className="object-cover opacity-20"
            sizes="100vw"
            priority={false}
          />
          <div className="absolute inset-0 bg-gradient-to-br from-[#0f4c2e]/95 via-brand-green-dark/95 to-brand-emerald/90" />
        </div>

        <div className="relative py-20">
          <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            <div className="text-center">
              <h2 className="text-3xl font-extrabold tracking-tight text-white sm:text-4xl">
                Our Impact in Numbers
              </h2>
              <p className="mx-auto mt-3 max-w-2xl text-lg text-green-100/70">
                Measurable progress towards a more sustainable procurement ecosystem
              </p>
            </div>

            <div className="mt-14 grid gap-8 sm:grid-cols-2 lg:grid-cols-4">
              {/* Counter 1: Verified Suppliers */}
              <div className="flex flex-col items-center gap-2 text-center">
                <div className="mb-2 flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 backdrop-blur-sm">
                  <Users className="h-7 w-7 text-green-300" />
                </div>
                <span className="animate-count-up text-5xl font-extrabold text-white">
                  50+
                </span>
                <span className="text-sm font-medium text-green-200/80">
                  Verified Suppliers
                </span>
              </div>

              {/* Counter 2: Green Industries */}
              <div className="flex flex-col items-center gap-2 text-center">
                <div className="mb-2 flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 backdrop-blur-sm">
                  <Factory className="h-7 w-7 text-green-300" />
                </div>
                <span className="animate-count-up text-5xl font-extrabold text-white" style={{ animationDelay: "0.15s" }}>
                  8
                </span>
                <span className="text-sm font-medium text-green-200/80">
                  Green Industries
                </span>
              </div>

              {/* Counter 3: African Countries */}
              <div className="flex flex-col items-center gap-2 text-center">
                <div className="mb-2 flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 backdrop-blur-sm">
                  <Globe className="h-7 w-7 text-green-300" />
                </div>
                <span className="animate-count-up text-5xl font-extrabold text-white" style={{ animationDelay: "0.3s" }}>
                  10
                </span>
                <span className="text-sm font-medium text-green-200/80">
                  African Countries
                </span>
              </div>

              {/* Counter 4: Transparency */}
              <div className="flex flex-col items-center gap-2 text-center">
                <div className="mb-2 flex h-14 w-14 items-center justify-center rounded-2xl bg-white/10 backdrop-blur-sm">
                  <Eye className="h-7 w-7 text-green-300" />
                </div>
                <span className="animate-count-up text-5xl font-extrabold text-white" style={{ animationDelay: "0.45s" }}>
                  100%
                </span>
                <span className="text-sm font-medium text-green-200/80">
                  Transparency
                </span>
              </div>
            </div>

            <p className="mt-12 text-center text-base font-medium text-green-100/60">
              Join the movement towards sustainable procurement in Africa
            </p>
          </div>
        </div>
      </section>

      {/* ================================================================= */}
      {/* 9. Browse by Industry (existing, moved down) */}
      {/* ================================================================= */}
      <section className="bg-gray-50 py-20">
        <div className="mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <div className="mb-3 inline-flex items-center gap-1.5 rounded-full bg-brand-green-light px-3 py-1 text-xs font-semibold text-brand-green">
              <Zap className="h-3.5 w-3.5" />
              Explore Sectors
            </div>
            <h2 className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl">
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
                className="group flex flex-col items-center gap-3 rounded-3xl border border-gray-100 bg-white p-6 text-center shadow-sm transition-all duration-300 ease-out hover:border-brand-green/20 hover:shadow-organic hover:-translate-y-1 hover:scale-105"
              >
                <div
                  className={`flex h-14 w-14 items-center justify-center rounded-2xl transition-transform duration-300 group-hover:scale-110 ${
                    industryIconBg[industry.slug] ?? "bg-gray-50 text-gray-600"
                  }`}
                >
                  <span
                    className="text-2xl"
                    dangerouslySetInnerHTML={{
                      __html: industryEmojis[industry.slug] ?? "&#127981;",
                    }}
                  />
                </div>
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

      {/* ================================================================= */}
      {/* 9b. Buyer CTA — Need Help Finding the Right Green Supplier? */}
      {/* ================================================================= */}
      <section className="py-12 bg-white">
        <div className="mx-auto max-w-5xl px-4 sm:px-6 lg:px-8">
          <div className="flex flex-col gap-6 rounded-2xl border-l-4 border-l-brand-green bg-white p-6 shadow-organic sm:p-8 lg:flex-row lg:items-center lg:justify-between">
            <div className="flex-1">
              <h2 className="text-xl font-extrabold tracking-tight text-gray-900 sm:text-2xl">
                Need Help Finding the Right Green Supplier?
              </h2>
              <p className="mt-2 max-w-xl text-base leading-relaxed text-gray-600">
                Tell us what you&apos;re looking for. Our team will match you with verified suppliers that meet your ESG requirements.
              </p>
              <p className="mt-3 text-sm text-gray-400">
                Free. No obligation. Response within 1 business day.
              </p>
            </div>
            <div className="shrink-0">
              <Link
                href="/get-listed"
                className="inline-flex items-center gap-2 rounded-2xl bg-brand-green px-6 py-3 text-sm font-semibold text-white shadow-md transition-all duration-300 ease-out hover:bg-brand-green-hover hover:shadow-lg hover:scale-[1.02]"
              >
                Submit a Supplier Request
                <ArrowRight className="h-4 w-4" />
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* ================================================================= */}
      {/* 10. CTA — Are You a Green Supplier? (existing) */}
      {/* ================================================================= */}
      <section className="relative">
        {/* SVG wave divider at top */}
        <div className="wave-divider relative -mb-px">
          <svg
            viewBox="0 0 1440 120"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            preserveAspectRatio="none"
            className="block w-full h-[60px] sm:h-[80px] md:h-[100px]"
          >
            <path
              d="M0,80 C240,120 480,40 720,80 C960,120 1200,40 1440,80 L1440,120 L0,120 Z"
              fill="#0f4c2e"
            />
          </svg>
        </div>

        <div className="bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald py-20">
          <div className="relative mx-auto max-w-7xl px-4 sm:px-6 lg:px-8">
            {/* Decorative SVG blobs */}
            <div className="pointer-events-none absolute inset-0 overflow-hidden">
              <svg
                className="absolute -top-16 -right-16 h-64 w-64 opacity-[0.06]"
                viewBox="0 0 300 300"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  d="M240,180Q220,240,160,260Q100,280,60,220Q20,160,60,100Q100,40,170,40Q240,40,250,110Q260,180,240,180Z"
                  fill="white"
                />
              </svg>
              <svg
                className="absolute -bottom-20 -left-20 h-80 w-80 opacity-[0.05]"
                viewBox="0 0 400 400"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  d="M320,240Q300,320,220,340Q140,360,80,290Q20,220,60,140Q100,60,190,50Q280,40,320,130Q360,220,320,240Z"
                  fill="white"
                />
              </svg>
            </div>

            <div className="relative text-center">
              <div className="mx-auto mb-6 flex h-14 w-14 items-center justify-center rounded-3xl bg-white/10">
                <Leaf className="h-7 w-7 text-green-300" />
              </div>
              <h2 className="text-3xl font-extrabold tracking-tight text-white sm:text-4xl">
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
                  className="inline-flex items-center gap-2 rounded-2xl bg-white px-6 py-3 text-sm font-semibold text-brand-green-dark shadow-lg transition-all duration-300 ease-out hover:bg-green-50 hover:shadow-xl hover:scale-[1.02]"
                >
                  Get Listed for Free
                  <ArrowRight className="h-4 w-4" />
                </Link>
                <Link
                  href="/verification"
                  className="inline-flex items-center gap-2 rounded-2xl border border-white/20 bg-white/10 px-6 py-3 text-sm font-semibold text-white backdrop-blur-sm transition-all duration-300 ease-out hover:bg-white/20"
                >
                  Learn About Verification
                </Link>
              </div>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
