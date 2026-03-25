import { SupplierCard } from "@/components/suppliers/supplier-card";
import type { SupplierSearchResult } from "@/lib/types";
import { SearchX, ChevronLeft, ChevronRight } from "lucide-react";
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
        <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
          <SearchX className="h-8 w-8 text-gray-400" />
        </div>
        <h3 className="text-lg font-semibold text-gray-900">
          No suppliers found
        </h3>
        <p className="mt-2 max-w-md text-sm text-gray-500">
          {query
            ? `No suppliers match "${query}". Try adjusting your search terms or filters.`
            : "No suppliers match your current filters. Try broadening your search criteria."}
        </p>
        <Link
          href="/suppliers"
          className="mt-6 inline-flex items-center rounded-xl bg-brand-green-light px-5 py-2.5 text-sm font-semibold text-brand-green-dark transition-colors hover:bg-green-100"
        >
          Clear all filters
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      {/* Results count */}
      <p className="text-sm text-gray-500">
        <span className="font-semibold text-gray-900">{total}</span>{" "}
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
    // We rely on the current URL search params being available via the link
    // Since this is a server component child, we build relative links
    // The page param will be merged by the search page
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
        <span className="inline-flex h-9 items-center gap-1 rounded-lg px-3 text-sm text-gray-300">
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
        <span className="inline-flex h-9 items-center gap-1 rounded-lg px-3 text-sm text-gray-300">
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
      className="inline-flex h-9 items-center gap-1 rounded-lg border border-gray-200 bg-white px-3 text-sm font-medium text-gray-700 transition-colors hover:border-brand-green hover:text-brand-green"
    >
      {children}
    </Link>
  );
}
