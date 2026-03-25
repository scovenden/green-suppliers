import Link from "next/link";
import { cn } from "@/lib/utils";
import type { SupplierSearchResult } from "@/lib/types";
import { getEsgBadgeColor } from "@/lib/types";
import { EsgBadge } from "./esg-badge";
import { CheckCircle, ArrowRight } from "lucide-react";

interface SupplierCardProps {
  supplier: SupplierSearchResult;
  className?: string;
}

function getInitials(name: string): string {
  return name
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w.charAt(0))
    .join("")
    .toUpperCase();
}

function getEsgHeaderColor(level: string): string {
  switch (level.toLowerCase()) {
    case "platinum":
      return "bg-gradient-to-r from-lime-600 to-green-700";
    case "gold":
      return "bg-gradient-to-r from-amber-500 to-amber-600";
    case "silver":
      return "bg-gradient-to-r from-gray-400 to-gray-500";
    case "bronze":
      return "bg-gradient-to-r from-amber-700 to-amber-800";
    default:
      return "bg-gradient-to-r from-gray-200 to-gray-300";
  }
}

export function SupplierCard({ supplier, className }: SupplierCardProps) {
  const esgColors = getEsgBadgeColor(supplier.esgLevel);
  const headerColor = getEsgHeaderColor(supplier.esgLevel);

  return (
    <Link
      href={`/suppliers/${supplier.slug}`}
      className={cn(
        "group flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition-all duration-200 hover:shadow-md hover:-translate-y-0.5",
        className
      )}
    >
      {/* ESG-colored header strip */}
      <div className={cn("h-2", headerColor)} />

      <div className="flex flex-col gap-4 p-5">
        {/* Top row: logo + name + badge */}
        <div className="flex items-start gap-3">
          {/* Company initials placeholder */}
          <div
            className={cn(
              "flex h-12 w-12 shrink-0 items-center justify-center rounded-xl text-sm font-bold",
              esgColors.bg,
              esgColors.text
            )}
          >
            {supplier.logoUrl ? (
              <img
                src={supplier.logoUrl}
                alt={supplier.tradingName}
                className="h-full w-full rounded-xl object-cover"
              />
            ) : (
              getInitials(supplier.tradingName)
            )}
          </div>

          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2">
              <h3 className="truncate text-base font-semibold text-gray-900 group-hover:text-brand-green">
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

        {/* View profile link */}
        <div className="flex items-center text-sm font-medium text-brand-green group-hover:text-brand-green-hover">
          View Profile
          <ArrowRight className="ml-1 h-4 w-4 transition-transform group-hover:translate-x-0.5" />
        </div>
      </div>
    </Link>
  );
}
