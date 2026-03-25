import type { Metadata } from "next";
import { notFound } from "next/navigation";
import Link from "next/link";
import { apiGet } from "@/lib/api-client";
import type { ContentPage } from "@/lib/types";
import { ArrowRight, BookOpen, Calendar } from "lucide-react";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface PageProps {
  params: Promise<{ slug: string }>;
}

// ---------------------------------------------------------------------------
// Fallback data
// ---------------------------------------------------------------------------

const fallbackGuides: Record<string, ContentPage> = {
  "what-is-esg-scoring": {
    id: "1",
    slug: "what-is-esg-scoring",
    title: "What Is ESG Scoring? A Guide for South African Businesses",
    metaTitle:
      "What Is ESG Scoring? A Guide for South African Businesses | Green Suppliers",
    metaDesc:
      "Learn how ESG scoring works, why it matters for South African procurement, and how Green Suppliers verifies supplier sustainability.",
    body: `<h2>Understanding ESG Scoring</h2>
<p>ESG scoring evaluates companies across three pillars: <strong>Environmental</strong>, <strong>Social</strong>, and <strong>Governance</strong>. For South African businesses, ESG is becoming critical for procurement decisions, regulatory compliance, and stakeholder trust.</p>

<h2>Why ESG Matters in South Africa</h2>
<p>South Africa faces unique environmental challenges including water scarcity, energy instability, and rapid urbanisation. Companies that prioritise ESG are better positioned to manage these risks and attract investment.</p>

<h3>Key Benefits</h3>
<ul>
<li>Improved access to green finance and investment</li>
<li>Compliance with JSE sustainability reporting requirements</li>
<li>Stronger supply chain resilience</li>
<li>Enhanced brand reputation among environmentally-conscious consumers</li>
</ul>

<h2>How Green Suppliers Scores ESG</h2>
<p>Our platform uses a transparent, rules-based scoring system with four tiers:</p>
<ul>
<li><strong>Bronze</strong> - Basic profile complete with all required company fields</li>
<li><strong>Silver</strong> - At least 1 valid certification and 20%+ renewable energy usage</li>
<li><strong>Gold</strong> - 2+ certifications, 50%+ renewable energy, and carbon reporting</li>
<li><strong>Platinum</strong> - 3+ certifications, 70%+ renewable energy, 70%+ waste recycling, and carbon reporting</li>
</ul>

<p>Scores are recalculated nightly and whenever certification status changes, ensuring accuracy and transparency.</p>

<h2>Getting Started</h2>
<p>Whether you are a buyer looking for verified green suppliers or a supplier wanting to showcase your credentials, Green Suppliers makes it easy. <a href="/get-listed">Get listed today</a> or <a href="/suppliers">browse our directory</a>.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-01-15T00:00:00Z",
    createdAt: "2026-01-15T00:00:00Z",
    updatedAt: "2026-03-20T00:00:00Z",
  },
  "green-procurement-south-africa": {
    id: "2",
    slug: "green-procurement-south-africa",
    title: "Green Procurement in South Africa: A Complete Guide",
    metaTitle:
      "Green Procurement in South Africa: A Complete Guide | Green Suppliers",
    metaDesc:
      "Everything you need to know about sustainable procurement practices in South Africa. Standards, regulations, and how to find verified green suppliers.",
    body: `<h2>What Is Green Procurement?</h2>
<p>Green procurement is the practice of purchasing products and services that have a reduced environmental impact compared to alternatives. In South Africa, this includes considering B-BBEE requirements alongside sustainability metrics.</p>

<h2>South African Green Procurement Standards</h2>
<p>Several frameworks guide green procurement in South Africa:</p>
<ul>
<li><strong>National Environmental Management Act (NEMA)</strong> - Foundation for environmental protection</li>
<li><strong>Green Building Council SA</strong> - Standards for sustainable construction</li>
<li><strong>ISO 14001</strong> - Environmental management systems certification</li>
<li><strong>Carbon Tax Act</strong> - Incentivises low-carbon supply chains</li>
</ul>

<h2>Finding Verified Green Suppliers</h2>
<p>Green Suppliers makes it easy to find and compare verified sustainable suppliers. Our platform offers:</p>
<ul>
<li>Transparent ESG scoring across all suppliers</li>
<li>Verified certifications with expiry tracking</li>
<li>Industry-specific filtering</li>
<li>Direct supplier inquiry forms</li>
</ul>

<p>Start your green procurement journey by <a href="/suppliers">browsing our directory</a>.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-02-10T00:00:00Z",
    createdAt: "2026-02-10T00:00:00Z",
    updatedAt: "2026-03-15T00:00:00Z",
  },
};

// ---------------------------------------------------------------------------
// Data fetching
// ---------------------------------------------------------------------------

async function getContentPage(slug: string): Promise<ContentPage | null> {
  try {
    const res = await apiGet<ContentPage>(`/content/${slug}`, {
      revalidate: 300,
    });
    if (res.success && res.data) {
      return res.data;
    }
  } catch {
    // API unreachable
  }

  // Fallback
  return fallbackGuides[slug] ?? null;
}

// ---------------------------------------------------------------------------
// Metadata
// ---------------------------------------------------------------------------

export async function generateMetadata({
  params,
}: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const page = await getContentPage(slug);

  if (!page) {
    return { title: "Guide Not Found | Green Suppliers" };
  }

  return {
    title: page.metaTitle ?? `${page.title} | Green Suppliers`,
    description:
      page.metaDesc ??
      `Read our guide on ${page.title}. Expert insights for sustainable procurement in South Africa.`,
    openGraph: {
      title: page.metaTitle ?? page.title,
      description: page.metaDesc ?? "",
      siteName: "Green Suppliers",
      type: "article",
    },
  };
}

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

export default async function GuidePage({ params }: PageProps) {
  const { slug } = await params;
  const page = await getContentPage(slug);

  if (!page) {
    notFound();
  }

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
              href="/guides"
              className="transition-colors hover:text-brand-green"
            >
              Guides
            </Link>
            <span>/</span>
            <span className="line-clamp-1 font-medium text-gray-900">
              {page.title}
            </span>
          </nav>
        </div>
      </div>

      {/* Article header */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6 sm:py-14 lg:px-8">
          <div className="flex flex-col gap-4">
            <div className="flex items-center gap-2">
              <span className="inline-flex items-center rounded-full bg-brand-green-light px-2.5 py-0.5 text-xs font-semibold text-brand-green-dark">
                Guide
              </span>
              {page.publishedAt && (
                <span className="flex items-center gap-1 text-xs text-gray-400">
                  <Calendar className="h-3.5 w-3.5" />
                  {formatDate(page.publishedAt)}
                </span>
              )}
            </div>
            <h1
              className="text-3xl font-extrabold tracking-tight text-gray-900 sm:text-4xl"
              style={{ letterSpacing: "-0.5px" }}
            >
              {page.title}
            </h1>
            {page.metaDesc && (
              <p className="text-lg leading-relaxed text-gray-500">
                {page.metaDesc}
              </p>
            )}
          </div>
        </div>
      </section>

      {/* Article body */}
      <article className="mx-auto max-w-3xl px-4 py-10 sm:px-6 lg:px-8">
        <div
          className="prose prose-gray prose-lg max-w-none prose-headings:font-extrabold prose-headings:tracking-tight prose-h2:text-2xl prose-h3:text-xl prose-a:text-brand-green prose-a:no-underline hover:prose-a:underline prose-strong:text-gray-900 prose-li:marker:text-brand-green"
          dangerouslySetInnerHTML={{ __html: page.body }}
        />
      </article>

      {/* CTAs */}
      <section className="border-t border-gray-100 bg-white">
        <div className="mx-auto max-w-3xl px-4 py-12 sm:px-6 lg:px-8">
          <div className="grid gap-4 sm:grid-cols-2">
            <Link
              href="/suppliers"
              className="group flex flex-col gap-2 rounded-2xl border border-gray-100 bg-gray-50 p-6 transition-all hover:border-brand-green/20 hover:shadow-md"
            >
              <h3 className="text-base font-semibold text-gray-900 group-hover:text-brand-green">
                Find Green Suppliers
              </h3>
              <p className="text-sm text-gray-500">
                Search and compare verified green suppliers across South Africa
                and beyond.
              </p>
              <span className="mt-auto flex items-center text-sm font-medium text-brand-green">
                Browse Directory
                <ArrowRight className="ml-1 h-4 w-4 transition-transform group-hover:translate-x-0.5" />
              </span>
            </Link>

            <Link
              href="/get-listed"
              className="group flex flex-col gap-2 rounded-2xl border border-gray-100 bg-gray-50 p-6 transition-all hover:border-brand-green/20 hover:shadow-md"
            >
              <h3 className="text-base font-semibold text-gray-900 group-hover:text-brand-green">
                Get Listed
              </h3>
              <p className="text-sm text-gray-500">
                Are you a green supplier? Showcase your ESG credentials and
                connect with enterprise buyers.
              </p>
              <span className="mt-auto flex items-center text-sm font-medium text-brand-green">
                Apply Now
                <ArrowRight className="ml-1 h-4 w-4 transition-transform group-hover:translate-x-0.5" />
              </span>
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}
