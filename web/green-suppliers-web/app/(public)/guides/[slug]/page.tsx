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
  "iso-14001-certification-guide": {
    id: "6",
    slug: "iso-14001-certification-guide",
    title: "ISO 14001 Certification: What Suppliers Need to Know",
    metaTitle:
      "ISO 14001 Certification: What Suppliers Need to Know | Green Suppliers",
    metaDesc:
      "A step-by-step guide to achieving ISO 14001 environmental management certification and how it impacts your ESG score.",
    body: `<h2>What Is ISO 14001?</h2>
<p>ISO 14001 is the international standard for environmental management systems. It helps organisations systematically manage their environmental impact, reduce waste, and improve sustainability performance.</p>

<h2>Why ISO 14001 Matters for Suppliers</h2>
<p>For suppliers looking to work with enterprise buyers, ISO 14001 certification is increasingly a baseline requirement. It demonstrates your commitment to environmental management and can significantly improve your ESG score on platforms like Green Suppliers.</p>

<h3>Benefits</h3>
<ul>
<li>Improved credibility with enterprise procurement teams</li>
<li>Higher ESG score on Green Suppliers (contributes to Silver tier and above)</li>
<li>Compliance with environmental regulations</li>
<li>Reduced costs through better resource management</li>
</ul>

<h2>The Certification Process</h2>
<ol>
<li><strong>Gap analysis</strong> — Assess current practices against ISO 14001 requirements</li>
<li><strong>EMS development</strong> — Design and document your environmental management system</li>
<li><strong>Implementation</strong> — Put the EMS into practice</li>
<li><strong>Internal audit</strong> — Verify compliance internally</li>
<li><strong>Certification audit</strong> — External assessment by an accredited body</li>
<li><strong>Surveillance audits</strong> — Annual checks to maintain certification</li>
</ol>

<p>Ready to improve your ESG score? <a href="/get-listed">Get listed on Green Suppliers</a> and showcase your certifications.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-02-28T00:00:00Z",
    createdAt: "2026-02-28T00:00:00Z",
    updatedAt: "2026-03-20T00:00:00Z",
  },
  "renewable-energy-for-business": {
    id: "7",
    slug: "renewable-energy-for-business",
    title: "Switching to Renewable Energy: A Business Guide",
    metaTitle:
      "Switching to Renewable Energy: A Business Guide | Green Suppliers",
    metaDesc:
      "How South African businesses can transition to renewable energy. Solar, wind, and battery storage options compared.",
    body: `<h2>Why Switch to Renewable Energy?</h2>
<p>South Africa's energy landscape is changing rapidly. Load shedding, rising electricity costs, and carbon tax obligations are pushing businesses to explore renewable energy alternatives. Beyond cost savings, renewable energy adoption directly improves your ESG score.</p>

<h2>Options for South African Businesses</h2>
<h3>Solar PV</h3>
<p>The most popular option for South African businesses. Rooftop and ground-mounted solar installations offer predictable energy costs and quick payback periods, typically 3-5 years.</p>

<h3>Wind Energy</h3>
<p>Viable for larger operations, particularly in the Western and Eastern Cape. Power purchase agreements (PPAs) allow businesses to benefit without owning the infrastructure.</p>

<h3>Battery Storage</h3>
<p>Increasingly affordable, battery storage allows businesses to store excess solar energy and reduce reliance on the grid during peak hours and load shedding events.</p>

<h2>Impact on ESG Score</h2>
<p>On Green Suppliers, renewable energy usage is a key factor in ESG scoring:</p>
<ul>
<li><strong>20%+ renewable energy</strong> — Required for Silver tier</li>
<li><strong>50%+ renewable energy</strong> — Required for Gold tier</li>
<li><strong>70%+ renewable energy</strong> — Required for Platinum tier</li>
</ul>

<p><a href="/esg-scoring">Learn more about our ESG scoring methodology</a> or <a href="/suppliers">find renewable energy suppliers</a> in our directory.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-03-05T00:00:00Z",
    createdAt: "2026-03-05T00:00:00Z",
    updatedAt: "2026-03-20T00:00:00Z",
  },
  "carbon-reporting-for-smes": {
    id: "8",
    slug: "carbon-reporting-for-smes",
    title: "Carbon Reporting for SMEs: Getting Started",
    metaTitle:
      "Carbon Reporting for SMEs: Getting Started | Green Suppliers",
    metaDesc:
      "A practical guide for small and medium enterprises to begin carbon reporting and work towards net-zero targets.",
    body: `<h2>Why Carbon Reporting Matters</h2>
<p>Carbon reporting is no longer just for large corporations. South Africa's Carbon Tax Act and increasing buyer scrutiny mean that SMEs need to understand and report their emissions. On Green Suppliers, carbon reporting is a requirement for Gold and Platinum ESG tiers.</p>

<h2>Getting Started</h2>
<h3>Step 1: Understand Your Scope</h3>
<p>Carbon emissions are typically measured in three scopes:</p>
<ul>
<li><strong>Scope 1:</strong> Direct emissions from your operations (fuel, company vehicles)</li>
<li><strong>Scope 2:</strong> Indirect emissions from purchased electricity</li>
<li><strong>Scope 3:</strong> All other indirect emissions (supply chain, business travel)</li>
</ul>
<p>Start with Scope 1 and 2 — they are the easiest to measure and the most impactful.</p>

<h3>Step 2: Collect Data</h3>
<p>Gather data on electricity consumption, fuel usage, and other emission sources. Your utility bills are a good starting point.</p>

<h3>Step 3: Calculate Emissions</h3>
<p>Use emission factors published by the South African Department of Forestry, Fisheries and the Environment to convert consumption data into CO2 equivalent (CO2e) figures.</p>

<h3>Step 4: Report and Set Targets</h3>
<p>Document your emissions baseline and set reduction targets. Many frameworks exist, including the GHG Protocol and CDP (formerly Carbon Disclosure Project).</p>

<h2>Impact on Your ESG Score</h2>
<p>On Green Suppliers, carbon reporting is a binary indicator — you either report or you do not. Reporting is required for Gold tier and above.</p>

<p><a href="/verification">Learn about our verification methodology</a> or <a href="/get-listed">get listed today</a>.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-03-18T00:00:00Z",
    createdAt: "2026-03-18T00:00:00Z",
    updatedAt: "2026-03-20T00:00:00Z",
  },
  "rise-of-green-procurement-south-africa": {
    id: "3",
    slug: "rise-of-green-procurement-south-africa",
    title: "The Rise of Green Procurement in South Africa",
    metaTitle:
      "The Rise of Green Procurement in South Africa | Green Suppliers",
    metaDesc:
      "How South African enterprises are reshaping their supply chains with ESG-compliant sourcing strategies and what this means for the broader African market.",
    body: `<h2>A Shifting Landscape</h2>
<p>South African enterprises are increasingly prioritising sustainability in their procurement decisions. Driven by regulatory pressure, investor expectations, and consumer demand, green procurement is no longer a nice-to-have — it is a strategic imperative.</p>

<h2>Key Drivers</h2>
<ul>
<li><strong>Regulatory compliance:</strong> The Carbon Tax Act and NEMA amendments are making ESG compliance mandatory for many industries.</li>
<li><strong>Investor pressure:</strong> JSE-listed companies face increasing scrutiny on their sustainability reporting and supply chain practices.</li>
<li><strong>Consumer expectations:</strong> South African consumers are becoming more environmentally conscious, driving demand for sustainable products.</li>
<li><strong>Cost savings:</strong> Energy efficiency and waste reduction translate directly to lower operating costs.</li>
</ul>

<h2>What This Means for Suppliers</h2>
<p>Suppliers who can demonstrate verified ESG credentials have a significant competitive advantage. Enterprise buyers are actively seeking partners who can provide evidence of sustainability practices, certifications, and carbon reporting.</p>

<h2>The Role of Green Suppliers</h2>
<p>Our platform makes it easy for buyers to discover and compare verified green suppliers. Every listing is independently assessed using our transparent ESG scoring methodology, giving procurement teams confidence in their sourcing decisions.</p>

<p><a href="/suppliers">Browse our directory</a> to find verified green suppliers, or <a href="/get-listed">get listed</a> to showcase your sustainability credentials.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-03-10T00:00:00Z",
    createdAt: "2026-03-10T00:00:00Z",
    updatedAt: "2026-03-20T00:00:00Z",
  },
  "iso-14001-certification-guide-african-businesses": {
    id: "4",
    slug: "iso-14001-certification-guide-african-businesses",
    title: "ISO 14001 Certification: A Complete Guide for African Businesses",
    metaTitle:
      "ISO 14001 Certification: A Complete Guide for African Businesses | Green Suppliers",
    metaDesc:
      "Everything you need to know about achieving ISO 14001 environmental management certification, from preparation to audit and beyond.",
    body: `<h2>What Is ISO 14001?</h2>
<p>ISO 14001 is the international standard for environmental management systems (EMS). It provides a framework for organisations to manage their environmental responsibilities in a systematic way, reduce waste, and improve resource efficiency.</p>

<h2>Why It Matters for African Businesses</h2>
<p>For businesses in Africa, ISO 14001 certification demonstrates a commitment to environmental stewardship. It is increasingly required by enterprise buyers, government tenders, and international partners as a baseline for environmental compliance.</p>

<h3>Key Benefits</h3>
<ul>
<li>Enhanced credibility with buyers and investors</li>
<li>Compliance with environmental regulations</li>
<li>Reduced operating costs through improved efficiency</li>
<li>Access to green procurement opportunities</li>
<li>Higher ESG score on platforms like Green Suppliers</li>
</ul>

<h2>The Certification Process</h2>
<ol>
<li><strong>Gap analysis:</strong> Assess your current environmental practices against ISO 14001 requirements.</li>
<li><strong>EMS development:</strong> Design and document your environmental management system.</li>
<li><strong>Implementation:</strong> Put the EMS into practice across your organisation.</li>
<li><strong>Internal audit:</strong> Verify compliance before the external audit.</li>
<li><strong>Certification audit:</strong> An accredited body conducts a formal assessment.</li>
<li><strong>Ongoing surveillance:</strong> Annual audits to maintain certification.</li>
</ol>

<h2>Impact on Your ESG Score</h2>
<p>On Green Suppliers, a valid ISO 14001 certification contributes directly to your ESG level. Combined with other sustainability practices, it can help you achieve Silver, Gold, or even Platinum status.</p>

<p><a href="/verification">Learn more about our verification methodology</a> or <a href="/get-listed">get listed today</a>.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-03-12T00:00:00Z",
    createdAt: "2026-03-12T00:00:00Z",
    updatedAt: "2026-03-20T00:00:00Z",
  },
  "esg-scoring-transforming-supplier-selection": {
    id: "5",
    slug: "esg-scoring-transforming-supplier-selection",
    title: "How ESG Scoring is Transforming Supplier Selection",
    metaTitle:
      "How ESG Scoring is Transforming Supplier Selection | Green Suppliers",
    metaDesc:
      "Data-driven sustainability scoring is replacing gut-feel procurement decisions. Learn how transparent ESG metrics are levelling the playing field.",
    body: `<h2>The Old Way vs. The New Way</h2>
<p>Traditionally, supplier selection relied heavily on price, delivery speed, and personal relationships. Sustainability was an afterthought — if it was considered at all. Today, ESG scoring is fundamentally changing how procurement teams evaluate and select suppliers.</p>

<h2>Why Data-Driven ESG Matters</h2>
<ul>
<li><strong>Objectivity:</strong> Rules-based scoring removes bias and ensures every supplier is measured against the same criteria.</li>
<li><strong>Transparency:</strong> Published methodologies mean suppliers know exactly what is being measured and how to improve.</li>
<li><strong>Comparability:</strong> Standardised scores make it easy to compare suppliers across industries and geographies.</li>
<li><strong>Accountability:</strong> Automatic recalculation and certification tracking ensure scores stay current.</li>
</ul>

<h2>How It Levels the Playing Field</h2>
<p>Small and medium enterprises often struggle to compete with larger companies on marketing and brand recognition. ESG scoring creates an objective playing field where a smaller supplier with strong sustainability practices can score higher than a larger competitor with weaker credentials.</p>

<h2>The Green Suppliers Approach</h2>
<p>Our platform uses a transparent, four-tier scoring system (Bronze, Silver, Gold, Platinum) that evaluates suppliers on certifications, renewable energy usage, waste recycling, and carbon reporting. Scores are recalculated nightly and visible to all buyers.</p>

<p><a href="/esg-scoring">Learn about our ESG scoring methodology</a> or <a href="/suppliers">browse scored suppliers</a>.</p>`,
    pageType: "guide",
    isPublished: true,
    publishedAt: "2026-03-15T00:00:00Z",
    createdAt: "2026-03-15T00:00:00Z",
    updatedAt: "2026-03-20T00:00:00Z",
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
