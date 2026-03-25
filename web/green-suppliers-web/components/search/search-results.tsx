import { SupplierCard } from "@/components/suppliers/supplier-card";
import type { SupplierSearchResult } from "@/lib/types";
import { SearchX, ChevronLeft, ChevronRight, Leaf } from "lucide-react";
import Link from "next/link";
import { cn } from "@/lib/utils";

interface SearchResultsProps {
  suppliers: SupplierSearchResult[];
  total: number;
  page: number;
  totalPages: number;
  query?: string;
}

export function SearchResults({
  suppliers,
  total,
  page,
  totalPages,
  query,
}: SearchResultsProps) {
  if (suppliers.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-center">
        {/* Illustration-style SVG leaf/plant */}
        <div className="mb-6 flex h-24 w-24 items-center justify-center rounded-3xl bg-brand-green-light">
          <svg
            width="48"
            height="48"
            viewBox="0 0 48 48"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
            className="text-brand-green"
          >
            <path
              d="M24 44V24"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
            />
            <path
              d="M24 24C24 24 12 22 8 12C8 12 18 8 24 18"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              fill="#F0FDF4"
            />
            <path
              d="M24 30C24 30 36 28 40 18C40 18 30 14 24 24"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              fill="#DCFCE7"
            />
            <path
              d="M24 36C24 36 14 34 10 26C10 26 20 22 24 30"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
              fill="#F0FDF4"
            />
          </svg>
        </div>

        <h3 className="text-lg font-semibold text-gray-900">
          No suppliers found
        </h3>
        <p className="mt-2 max-w-md text-sm text-gray-500">
          {query
            ? `No suppliers match "${query}". Try adjusting your search terms or filters.`
            : "No suppliers match your current filters. Try broadening your search criteria."}
        </p>

        {/* Helpful suggestions */}
        <div className="mt-6 flex flex-col items-center gap-3">
          <p className="text-xs font-medium uppercase tracking-wider text-gray-400">
            Try these instead
          </p>
          <div className="flex flex-wrap justify-center gap-2">
            {["Solar Energy", "Waste Management", "Water Solutions", "ISO 14001"].map((suggestion) => (
              <Link
                key={suggestion}
                href={`/suppliers?q=${encodeURIComponent(suggestion)}`}
                className="rounded-full border border-brand-green/20 bg-brand-green-light px-3 py-1 text-xs font-medium text-brand-green-dark transition-all duration-300 ease-out hover:bg-green-100 hover:border-brand-green/40"
              >
                {suggestion}
              </Link>
            ))}
          </div>
        </div>

        <Link
          href="/suppliers"
          className="mt-6 inline-flex items-center rounded-2xl bg-brand-green-light px-5 py-2.5 text-sm font-semibold text-brand-green-dark transition-all duration-300 ease-out hover:bg-green-100 hover:shadow-organic"
        >
          Clear all filters
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Results count with animated counter feel */}
      <p className="text-sm text-gray-500">
        <span className="animate-count-up inline-block font-semibold text-gray-900">{total}</span>{" "}
        supplier{total !== 1 ? "s" : ""} found
        {query ? (
          <>
            {" "}
            for{" "}
            <span className="font-medium text-gray-700">
              &ldquo;{query}&rdquo;
            </span>
          </>
        ) : null}
      </p>

      {/* Results grid */}
      <div className="grid gap-5 sm:grid-cols-2">
        {suppliers.map((supplier) => (
          <SupplierCard key={supplier.id} supplier={supplier} />
        ))}
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <Pagination page={page} totalPages={totalPages} />
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Skeleton loading cards
// ---------------------------------------------------------------------------

export function SearchResultsSkeleton() {
  return (
    <div className="flex flex-col gap-6">
      {/* Count skeleton */}
      <div className="h-5 w-40 skeleton-green rounded-lg" />

      {/* Card skeletons */}
      <div className="grid gap-5 sm:grid-cols-2">
        {Array.from({ length: 6 }).map((_, i) => (
          <div
            key={i}
            className="flex flex-col overflow-hidden rounded-[20px] border border-gray-100 bg-white"
          >
            <div className="h-1.5 skeleton-green" />
            <div className="flex flex-col gap-4 p-5">
              {/* Header row */}
              <div className="flex items-start gap-3">
                <div className="h-12 w-12 shrink-0 rounded-2xl skeleton-green" />
                <div className="flex-1 space-y-2">
                  <div className="h-4 w-3/4 skeleton-green rounded" />
                  <div className="h-3 w-1/2 skeleton-green rounded" />
                </div>
                <div className="h-6 w-16 skeleton-green rounded-full" />
              </div>
              {/* Description */}
              <div className="space-y-2">
                <div className="h-3 w-full skeleton-green rounded" />
                <div className="h-3 w-4/5 skeleton-green rounded" />
              </div>
              {/* Tags */}
              <div className="flex gap-2">
                <div className="h-5 w-20 skeleton-green rounded-full" />
                <div className="h-5 w-16 skeleton-green rounded-full" />
              </div>
              {/* Bottom */}
              <div className="h-4 w-24 skeleton-green rounded" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

// ---------------------------------------------------------------------------
// Pagination
// ---------------------------------------------------------------------------

function Pagination({
  page,
  totalPages,
}: {
  page: number;
  totalPages: number;
}) {
  function buildHref(targetPage: number): string {
    return `?page=${targetPage}`;
  }

  return (
    <nav
      aria-label="Pagination"
      className="flex items-center justify-center gap-2 pt-4"
    >
      {page > 1 ? (
        <PaginationLink href={buildHref(page - 1)} label="Previous page">
          <ChevronLeft className="h-4 w-4" />
          <span className="hidden sm:inline">Previous</span>
        </PaginationLink>
      ) : (
        <span className="inline-flex h-9 items-center gap-1 rounded-xl px-3 text-sm text-gray-300">
          <ChevronLeft className="h-4 w-4" />
          <span className="hidden sm:inline">Previous</span>
        </span>
      )}

      <span className="px-3 text-sm text-gray-600">
        Page <span className="font-semibold text-gray-900">{page}</span> of{" "}
        <span className="font-semibold text-gray-900">{totalPages}</span>
      </span>

      {page < totalPages ? (
        <PaginationLink href={buildHref(page + 1)} label="Next page">
          <span className="hidden sm:inline">Next</span>
          <ChevronRight className="h-4 w-4" />
        </PaginationLink>
      ) : (
        <span className="inline-flex h-9 items-center gap-1 rounded-xl px-3 text-sm text-gray-300">
          <span className="hidden sm:inline">Next</span>
          <ChevronRight className="h-4 w-4" />
        </span>
      )}
    </nav>
  );
}

function PaginationLink({
  href,
  label,
  children,
}: {
  href: string;
  label: string;
  children: React.ReactNode;
}) {
  return (
    <Link
      href={href}
      aria-label={label}
      className="inline-flex h-9 items-center gap-1 rounded-xl border border-gray-200 bg-white px-3 text-sm font-medium text-gray-700 transition-all duration-300 ease-out hover:border-brand-green hover:text-brand-green hover:shadow-sm"
    >
      {children}
    </Link>
  );
}
