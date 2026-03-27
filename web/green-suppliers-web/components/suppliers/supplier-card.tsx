import Image from "next/image";
import Link from "next/link";
import { cn } from "@/lib/utils";
import type { SupplierSearchResult } from "@/lib/types";
import { getEsgBadgeColor } from "@/lib/types";
import { EsgBadge } from "./esg-badge";
import { SaveSupplierButton } from "./save-supplier-button";
import { CheckCircle, ArrowRight, Shield } from "lucide-react";

interface SupplierCardProps {
  supplier: SupplierSearchResult;
  className?: string;
  /** If provided, shows a save/bookmark button for authenticated buyers */
  buyerAuth?: {
    token: string;
    savedId?: string | null;
  };
}

function getInitials(name: string): string {
  return name
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w.charAt(0))
    .join("")
    .toUpperCase();
}

function getEsgGradientBorder(level: string): string {
  switch (level.toLowerCase()) {
    case "platinum":
      return "from-lime-500 via-green-600 to-emerald-700";
    case "gold":
      return "from-amber-400 via-amber-500 to-amber-600";
    case "silver":
      return "from-gray-300 via-gray-400 to-gray-500";
    case "bronze":
      return "from-amber-600 via-amber-700 to-amber-800";
    default:
      return "from-gray-200 via-gray-250 to-gray-300";
  }
}

export function SupplierCard({ supplier, className, buyerAuth }: SupplierCardProps) {
  const esgColors = getEsgBadgeColor(supplier.esgLevel);
  const gradientBorder = getEsgGradientBorder(supplier.esgLevel);

  // Simulate a renewable energy % from esgScore for the mini-bar visual
  const renewablePercent = Math.min(100, Math.max(0, supplier.esgScore));

  return (
    <Link
      href={`/suppliers/${supplier.slug}`}
      className={cn(
        "group relative flex flex-col overflow-hidden rounded-3xl border border-gray-100 bg-white transition-all duration-300 ease-out",
        "shadow-[0_4px_16px_rgba(0,0,0,0.06)]",
        "hover:shadow-[0_12px_48px_rgba(0,0,0,0.12)] hover:-translate-y-1",
        className
      )}
    >
      {/* ESG-colored gradient border strip at top */}
      <div className={cn("h-1.5 bg-gradient-to-r", gradientBorder)} />

      {/* Save supplier button for authenticated buyers */}
      {buyerAuth && (
        <div className="absolute right-3 top-5 z-10">
          <SaveSupplierButton
            supplierProfileId={supplier.id}
            savedId={buyerAuth.savedId}
            token={buyerAuth.token}
          />
        </div>
      )}

      <div className="flex flex-col gap-4 p-5">
        {/* Top row: logo + name + badge */}
        <div className="flex items-start gap-3">
          {/* Company initials placeholder */}
          <div
            className={cn(
              "flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl text-sm font-bold transition-transform duration-300 ease-out group-hover:scale-105",
              esgColors.bg,
              esgColors.text
            )}
          >
            {supplier.logoUrl ? (
              <Image
                src={supplier.logoUrl}
                alt={supplier.tradingName}
                width={48}
                height={48}
                className="h-full w-full rounded-2xl object-cover"
              />
            ) : (
              getInitials(supplier.tradingName)
            )}
          </div>

          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <h3 className="truncate text-base font-semibold text-gray-900 transition-colors duration-300 group-hover:text-brand-green">
                {supplier.tradingName}
              </h3>
              {supplier.isVerified && (
                <CheckCircle className="h-4 w-4 shrink-0 text-brand-green" />
              )}
            </div>
            <p className="text-sm text-gray-500">
              {supplier.city && `${supplier.city}, `}
              {supplier.countryCode}
            </p>
          </div>

          <EsgBadge level={supplier.esgLevel} />
        </div>

        {/* Description */}
        {supplier.shortDescription && (
          <p className="line-clamp-2 text-sm leading-relaxed text-gray-600">
            {supplier.shortDescription}
          </p>
        )}

        {/* Industry tags */}
        {supplier.industries.length > 0 && (
          <div className="flex flex-wrap gap-1.5">
            {supplier.industries.slice(0, 3).map((industry) => (
              <span
                key={industry}
                className="inline-flex items-center rounded-full bg-brand-green-light px-2.5 py-0.5 text-xs font-medium text-brand-green-dark"
              >
                {industry}
              </span>
            ))}
            {supplier.industries.length > 3 && (
              <span className="inline-flex items-center rounded-full bg-gray-100 px-2.5 py-0.5 text-xs font-medium text-gray-500">
                +{supplier.industries.length - 3}
              </span>
            )}
          </div>
        )}

        {/* Certification count + Sustainability mini-bar */}
        <div className="flex items-center gap-4">
          {/* Certification count */}
          {supplier.isVerified && (
            <div className="flex items-center gap-1.5 text-xs text-gray-500">
              <Shield className="h-3.5 w-3.5 text-harvest-gold" />
              <span className="font-medium">
                {supplier.esgLevel.toLowerCase() === "platinum"
                  ? "3+ certs"
                  : supplier.esgLevel.toLowerCase() === "gold"
                  ? "2+ certs"
                  : "1+ cert"}
              </span>
            </div>
          )}

          {/* Renewable energy mini-bar */}
          <div className="flex flex-1 items-center gap-2">
            <span className="text-xs text-gray-400 whitespace-nowrap">ESG</span>
            <div className="relative h-1.5 flex-1 overflow-hidden rounded-full bg-gray-100">
              <div
                className="absolute inset-y-0 left-0 rounded-full bg-gradient-to-r from-brand-green to-brand-emerald transition-all duration-500 ease-out"
                style={{ width: `${renewablePercent}%` }}
              />
            </div>
            <span className="text-xs font-medium text-gray-500">
              {renewablePercent}
            </span>
          </div>
        </div>

        {/* View profile link */}
        <div className="flex items-center text-sm font-medium text-brand-green transition-colors duration-300 group-hover:text-brand-green-hover">
          View Profile
          <ArrowRight className="ml-1 h-4 w-4 transition-transform duration-300 group-hover:translate-x-1" />
        </div>
      </div>
    </Link>
  );
}
