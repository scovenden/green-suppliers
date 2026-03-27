import type { Metadata } from "next";
import Image from "next/image";
import { notFound } from "next/navigation";
import { apiGet } from "@/lib/api-client";
import type { SupplierProfile, CertificationDto } from "@/lib/types";
import { getEsgBadgeColor } from "@/lib/types";
import { EsgBadge } from "@/components/suppliers/esg-badge";
import { LeadForm } from "@/components/leads/lead-form";
import { cn } from "@/lib/utils";
import {
  CheckCircle,
  MapPin,
  Calendar,
  Users,
  Zap,
  Recycle,
  FileText,
  Droplets,
  Package,
  Globe,
  Shield,
  Award,
  ArrowLeft,
  ExternalLink,
} from "lucide-react";
import Link from "next/link";

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

interface PageProps {
  params: Promise<{ slug: string }>;
}

// ---------------------------------------------------------------------------
// Data fetching
// ---------------------------------------------------------------------------

async function getSupplier(slug: string): Promise<SupplierProfile | null> {
  try {
    const res = await apiGet<SupplierProfile>(`/suppliers/${slug}`, {
      revalidate: 120,
    });
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

export async function generateMetadata({ params }: PageProps): Promise<Metadata> {
  const { slug } = await params;
  const res = await apiGet<SupplierProfile>(`/suppliers/${slug}`);
  const supplier = res.data;
  if (!supplier) return { title: "Supplier Not Found" };
  return {
    title: `${supplier.tradingName || supplier.organizationName} | Green Suppliers`,
    description:
      supplier.shortDescription || supplier.description?.slice(0, 155),
    openGraph: {
      title: supplier.tradingName || supplier.organizationName,
      description: supplier.shortDescription || "",
      images: supplier.logoUrl ? [supplier.logoUrl] : [],
    },
  };
}

// ---------------------------------------------------------------------------
// Page component
// ---------------------------------------------------------------------------

export default async function SupplierProfilePage({ params }: PageProps) {
  const { slug } = await params;
  const supplier = await getSupplier(slug);

  if (!supplier) {
    notFound();
  }

  const esgColors = getEsgBadgeColor(supplier.esgLevel);
  const isVerified = supplier.verificationStatus === "Verified";

  return (
    <div className="min-h-screen bg-gray-50/50">
      {/* Back link */}
      <div className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-3 sm:px-6 lg:px-8">
          <Link
            href="/suppliers"
            className="inline-flex items-center gap-1.5 text-sm text-gray-500 transition-colors hover:text-brand-green"
          >
            <ArrowLeft className="h-4 w-4" />
            Back to Search
          </Link>
        </div>
      </div>

      {/* Hero banner */}
      <section className="border-b border-gray-100 bg-white">
        <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 sm:py-12 lg:px-8">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-start lg:gap-8">
            {/* Logo / initials */}
            <div
              className={cn(
                "flex h-20 w-20 shrink-0 items-center justify-center rounded-2xl text-xl font-bold shadow-sm",
                esgColors.bg,
                esgColors.text
              )}
            >
              {supplier.logoUrl ? (
                <Image
                  src={supplier.logoUrl}
                  alt={supplier.tradingName}
                  width={80}
                  height={80}
                  className="h-full w-full rounded-2xl object-cover"
                />
              ) : (
                getInitials(supplier.tradingName || supplier.organizationName)
              )}
            </div>

            <div className="flex-1">
              <div className="flex flex-wrap items-center gap-3">
                <h1 className="text-2xl font-extrabold tracking-tight text-gray-900 sm:text-3xl">
                  {supplier.tradingName || supplier.organizationName}
                </h1>
                {isVerified && (
                  <span className="inline-flex items-center gap-1 rounded-full bg-brand-green-light px-2.5 py-0.5 text-xs font-semibold text-brand-green-dark">
                    <CheckCircle className="h-3.5 w-3.5" />
                    Verified
                  </span>
                )}
                <EsgBadge level={supplier.esgLevel} />
              </div>

              {supplier.tradingName &&
                supplier.tradingName !== supplier.organizationName && (
                  <p className="mt-1 text-sm text-gray-500">
                    Legal name: {supplier.organizationName}
                  </p>
                )}

              <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-gray-500">
                {(supplier.city || supplier.province) && (
                  <span className="inline-flex items-center gap-1">
                    <MapPin className="h-4 w-4" />
                    {[supplier.city, supplier.province, supplier.countryCode]
                      .filter(Boolean)
                      .join(", ")}
                  </span>
                )}
                {supplier.yearFounded && (
                  <span className="inline-flex items-center gap-1">
                    <Calendar className="h-4 w-4" />
                    Founded {supplier.yearFounded}
                  </span>
                )}
                {supplier.website && (
                  <a
                    href={
                      supplier.website.startsWith("http")
                        ? supplier.website
                        : `https://${supplier.website}`
                    }
                    target="_blank"
                    rel="noopener noreferrer"
                    className="inline-flex items-center gap-1 text-brand-green hover:underline"
                  >
                    <Globe className="h-4 w-4" />
                    Website
                    <ExternalLink className="h-3 w-3" />
                  </a>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* Main content */}
      <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="grid gap-8 lg:grid-cols-[1fr_380px]">
          {/* Left column */}
          <div className="flex flex-col gap-8">
            {/* Quick stats */}
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <StatCard
                icon={<Users className="h-5 w-5 text-brand-green" />}
                label="Employees"
                value={supplier.employeeCount ?? "N/A"}
              />
              <StatCard
                icon={<Zap className="h-5 w-5 text-amber-500" />}
                label="Renewable Energy"
                value={
                  supplier.renewableEnergyPercent != null
                    ? `${supplier.renewableEnergyPercent}%`
                    : "N/A"
                }
              />
              <StatCard
                icon={<Recycle className="h-5 w-5 text-emerald-500" />}
                label="Waste Recycling"
                value={
                  supplier.wasteRecyclingPercent != null
                    ? `${supplier.wasteRecyclingPercent}%`
                    : "N/A"
                }
              />
              <StatCard
                icon={<FileText className="h-5 w-5 text-blue-500" />}
                label="Carbon Reporting"
                value={supplier.carbonReporting ? "Yes" : "No"}
              />
            </div>

            {/* About section */}
            {supplier.description && (
              <section className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                <h2 className="text-lg font-semibold text-gray-900">About</h2>
                <div className="mt-3 whitespace-pre-line text-sm leading-relaxed text-gray-600">
                  {supplier.description}
                </div>
              </section>
            )}

            {/* Additional sustainability details */}
            {(supplier.waterManagement || supplier.sustainablePackaging) && (
              <section className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                <h2 className="text-lg font-semibold text-gray-900">
                  Sustainability Practices
                </h2>
                <div className="mt-3 flex flex-wrap gap-3">
                  {supplier.waterManagement && (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-blue-50 px-3 py-1.5 text-xs font-medium text-blue-700">
                      <Droplets className="h-3.5 w-3.5" />
                      Water Management
                    </span>
                  )}
                  {supplier.sustainablePackaging && (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-green-50 px-3 py-1.5 text-xs font-medium text-green-700">
                      <Package className="h-3.5 w-3.5" />
                      Sustainable Packaging
                    </span>
                  )}
                  {supplier.carbonReporting && (
                    <span className="inline-flex items-center gap-1.5 rounded-full bg-purple-50 px-3 py-1.5 text-xs font-medium text-purple-700">
                      <FileText className="h-3.5 w-3.5" />
                      Carbon Reporting
                    </span>
                  )}
                </div>
              </section>
            )}

            {/* Industries & Services */}
            {(supplier.industries.length > 0 ||
              supplier.serviceTags.length > 0) && (
              <section className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                <h2 className="text-lg font-semibold text-gray-900">
                  Industries & Services
                </h2>
                {supplier.industries.length > 0 && (
                  <div className="mt-3">
                    <h3 className="text-xs font-semibold uppercase tracking-wider text-gray-400">
                      Industries
                    </h3>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {supplier.industries.map((ind) => (
                        <Link
                          key={ind.id}
                          href={`/industries/${ind.slug}`}
                          className="inline-flex items-center rounded-full bg-brand-green-light px-3 py-1 text-xs font-medium text-brand-green-dark transition-colors hover:bg-green-100"
                        >
                          {ind.name}
                        </Link>
                      ))}
                    </div>
                  </div>
                )}
                {supplier.serviceTags.length > 0 && (
                  <div className="mt-4">
                    <h3 className="text-xs font-semibold uppercase tracking-wider text-gray-400">
                      Services
                    </h3>
                    <div className="mt-2 flex flex-wrap gap-2">
                      {supplier.serviceTags.map((tag) => (
                        <span
                          key={tag.id}
                          className="inline-flex items-center rounded-full bg-gray-100 px-3 py-1 text-xs font-medium text-gray-600"
                        >
                          {tag.name}
                        </span>
                      ))}
                    </div>
                  </div>
                )}
              </section>
            )}

            {/* Certifications */}
            {supplier.certifications.length > 0 && (
              <section className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
                <h2 className="text-lg font-semibold text-gray-900">
                  Certifications
                </h2>
                <div className="mt-4 space-y-3">
                  {supplier.certifications.map((cert) => (
                    <CertificationCard key={cert.id} cert={cert} />
                  ))}
                </div>
              </section>
            )}

            {/* ESG Methodology */}
            <section className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
              <h2 className="text-lg font-semibold text-gray-900">
                ESG Scoring Methodology
              </h2>
              <p className="mt-3 text-sm leading-relaxed text-gray-600">
                Our ESG levels are determined by a rules-based scoring system
                that evaluates suppliers across multiple sustainability
                dimensions. The score is recalculated nightly and whenever
                certification status changes.
              </p>
              <div className="mt-4 space-y-3">
                <EsgLevelExplainer
                  level="Bronze"
                  description="Basic profile complete with all required company fields."
                  color="bg-gradient-to-r from-amber-700 to-amber-800"
                  active={supplier.esgLevel.toLowerCase() === "bronze"}
                />
                <EsgLevelExplainer
                  level="Silver"
                  description="At least 1 valid certification and 20%+ renewable energy."
                  color="bg-gradient-to-r from-gray-400 to-gray-500"
                  active={supplier.esgLevel.toLowerCase() === "silver"}
                />
                <EsgLevelExplainer
                  level="Gold"
                  description="2+ valid certifications, 50%+ renewable energy, and carbon reporting."
                  color="bg-gradient-to-r from-amber-500 to-amber-600"
                  active={supplier.esgLevel.toLowerCase() === "gold"}
                />
                <EsgLevelExplainer
                  level="Platinum"
                  description="3+ valid certifications, 70%+ renewable energy, 70%+ waste recycling, and carbon reporting."
                  color="bg-gradient-to-r from-lime-600 to-green-700"
                  active={supplier.esgLevel.toLowerCase() === "platinum"}
                />
              </div>
            </section>
          </div>

          {/* Right column — Lead form */}
          <div className="lg:sticky lg:top-24">
            <LeadForm
              supplierProfileId={supplier.id}
              supplierName={
                supplier.tradingName || supplier.organizationName
              }
            />
          </div>
        </div>
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Helper components
// ---------------------------------------------------------------------------

function getInitials(name: string): string {
  return name
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w.charAt(0))
    .join("")
    .toUpperCase();
}

function StatCard({
  icon,
  label,
  value,
}: {
  icon: React.ReactNode;
  label: string;
  value: string;
}) {
  return (
    <div className="flex items-center gap-3 rounded-2xl border border-gray-100 bg-white p-4 shadow-sm">
      <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-gray-50">
        {icon}
      </div>
      <div>
        <p className="text-xs text-gray-500">{label}</p>
        <p className="text-base font-semibold text-gray-900">{value}</p>
      </div>
    </div>
  );
}

function CertificationCard({ cert }: { cert: CertificationDto }) {
  const statusConfig = getCertStatusConfig(cert.status);

  return (
    <div className="flex items-center justify-between rounded-xl border border-gray-100 bg-gray-50/50 px-4 py-3">
      <div className="flex items-center gap-3">
        <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-white shadow-sm">
          <Award className="h-4.5 w-4.5 text-brand-green" />
        </div>
        <div>
          <p className="text-sm font-medium text-gray-900">
            {cert.certTypeName}
          </p>
          {cert.certificateNumber && (
            <p className="text-xs text-gray-500">#{cert.certificateNumber}</p>
          )}
        </div>
      </div>
      <div className="flex flex-col items-end gap-1">
        <span
          className={cn(
            "inline-flex items-center rounded-full px-2 py-0.5 text-xs font-medium",
            statusConfig.className
          )}
        >
          {statusConfig.label}
        </span>
        {cert.expiresAt && (
          <span className="text-xs text-gray-400">
            Expires {formatDate(cert.expiresAt)}
          </span>
        )}
      </div>
    </div>
  );
}

function getCertStatusConfig(status: string): {
  label: string;
  className: string;
} {
  switch (status.toLowerCase()) {
    case "accepted":
    case "active":
      return {
        label: "Active",
        className: "bg-green-100 text-green-700",
      };
    case "pending":
      return {
        label: "Pending",
        className: "bg-yellow-100 text-yellow-700",
      };
    case "expired":
      return {
        label: "Expired",
        className: "bg-red-100 text-red-700",
      };
    case "rejected":
      return {
        label: "Rejected",
        className: "bg-red-100 text-red-700",
      };
    default:
      return {
        label: status,
        className: "bg-gray-100 text-gray-600",
      };
  }
}

function formatDate(dateStr: string): string {
  try {
    return new Date(dateStr).toLocaleDateString("en-ZA", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  } catch {
    return dateStr;
  }
}

function EsgLevelExplainer({
  level,
  description,
  color,
  active,
}: {
  level: string;
  description: string;
  color: string;
  active: boolean;
}) {
  return (
    <div
      className={cn(
        "flex items-start gap-3 rounded-xl border px-4 py-3 transition-colors",
        active
          ? "border-brand-green/30 bg-brand-green-light"
          : "border-gray-100 bg-gray-50/50"
      )}
    >
      <span
        className={cn(
          "mt-0.5 inline-block h-3 w-3 shrink-0 rounded-full",
          color
        )}
      />
      <div>
        <p
          className={cn(
            "text-sm font-semibold",
            active ? "text-brand-green-dark" : "text-gray-700"
          )}
        >
          {level}
          {active && (
            <span className="ml-2 text-xs font-normal text-brand-green">
              (Current)
            </span>
          )}
        </p>
        <p className="mt-0.5 text-xs text-gray-500">{description}</p>
      </div>
    </div>
  );
}
