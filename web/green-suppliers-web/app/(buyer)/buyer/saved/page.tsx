"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiDelete } from "@/lib/api-client";
import type { SavedSupplier } from "@/lib/types";
import { getEsgBadgeColor } from "@/lib/types";
import { EsgBadge } from "@/components/suppliers/esg-badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  CheckCircle,
  ArrowRight,
  X,
  Bookmark,
  Search,
  AlertTriangle,
  Loader2,
  Shield,
} from "lucide-react";
import { cn } from "@/lib/utils";

function getInitials(name: string): string {
  return name
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w.charAt(0))
    .join("")
    .toUpperCase();
}

function SavedSupplierCard({
  supplier,
  onRemove,
  removing,
}: {
  supplier: SavedSupplier;
  onRemove: (id: string) => void;
  removing: boolean;
}) {
  const esgColors = getEsgBadgeColor(supplier.esgLevel);
  const renewablePercent = Math.min(100, Math.max(0, supplier.esgScore));

  return (
    <div className="group relative flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition-all hover:shadow-md">
      {/* Remove button */}
      <button
        onClick={(e) => {
          e.preventDefault();
          e.stopPropagation();
          onRemove(supplier.id);
        }}
        disabled={removing}
        className="absolute right-3 top-3 z-10 flex h-8 w-8 items-center justify-center rounded-full bg-white/80 text-muted-foreground shadow-sm backdrop-blur-sm transition-colors hover:bg-red-50 hover:text-red-600 disabled:opacity-50"
        aria-label={`Remove ${supplier.tradingName} from saved`}
      >
        {removing ? (
          <Loader2 className="h-4 w-4 animate-spin" />
        ) : (
          <X className="h-4 w-4" />
        )}
      </button>

      <Link href={`/suppliers/${supplier.slug}`} className="flex flex-1 flex-col">
        <div className="flex flex-col gap-4 p-5">
          {/* Top row: logo + name + badge */}
          <div className="flex items-start gap-3">
            <div
              className={cn(
                "flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl text-sm font-bold",
                esgColors.bg,
                esgColors.text
              )}
            >
              {supplier.logoUrl ? (
                <img
                  src={supplier.logoUrl}
                  alt={supplier.tradingName}
                  className="h-full w-full rounded-2xl object-cover"
                />
              ) : (
                getInitials(supplier.tradingName)
              )}
            </div>

            <div className="min-w-0 flex-1">
              <div className="flex items-center gap-2">
                <h3 className="truncate text-base font-semibold text-gray-900 transition-colors group-hover:text-brand-green">
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

          {/* ESG bar + cert count */}
          <div className="flex items-center gap-4">
            {supplier.isVerified && (
              <div className="flex items-center gap-1.5 text-xs text-gray-500">
                <Shield className="h-3.5 w-3.5 text-harvest-gold" />
                <span className="font-medium">Verified</span>
              </div>
            )}
            <div className="flex flex-1 items-center gap-2">
              <span className="whitespace-nowrap text-xs text-gray-400">
                ESG
              </span>
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
          <div className="flex items-center text-sm font-medium text-brand-green transition-colors group-hover:text-brand-green-hover">
            View Profile
            <ArrowRight className="ml-1 h-4 w-4 transition-transform group-hover:translate-x-1" />
          </div>
        </div>
      </Link>
    </div>
  );
}

function GridSkeleton() {
  return (
    <div
      role="status"
      aria-label="Loading saved suppliers"
      className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3"
    >
      {Array.from({ length: 6 }).map((_, i) => (
        <Skeleton key={i} className="h-64 rounded-2xl" />
      ))}
      <span className="sr-only">Loading saved suppliers</span>
    </div>
  );
}

export default function SavedSuppliersPage() {
  const { token } = useAuth();
  const [suppliers, setSuppliers] = useState<SavedSupplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [removingId, setRemovingId] = useState<string | null>(null);

  const fetchSaved = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    const res = await apiGetAuth<SavedSupplier[]>(
      "/buyer/me/saved-suppliers",
      token
    );
    if (res.success && res.data) {
      setSuppliers(res.data);
    } else {
      setError(res.error?.message ?? "Failed to load saved suppliers");
    }
    setLoading(false);
  }, [token]);

  useEffect(() => {
    fetchSaved();
  }, [fetchSaved]);

  async function handleRemove(id: string) {
    if (!token) return;
    setRemovingId(id);

    const res = await apiDelete(`/buyer/me/saved-suppliers/${id}`, token);
    if (res.success) {
      setSuppliers((prev) => prev.filter((s) => s.id !== id));
      toast.success("Supplier removed from saved list");
    } else {
      toast.error(res.error?.message ?? "Failed to remove supplier");
    }
    setRemovingId(null);
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">
            Saved Suppliers
          </h1>
          <p className="text-sm text-muted-foreground">
            Your shortlisted suppliers for easy access
          </p>
        </div>
        <Link href="/suppliers">
          <Button variant="outline" size="sm">
            <Search className="mr-1 h-4 w-4" />
            Browse Directory
          </Button>
        </Link>
      </div>

      {loading ? (
        <GridSkeleton />
      ) : error ? (
        <div
          role="alert"
          className="flex flex-col items-center justify-center gap-4 py-20"
        >
          <AlertTriangle
            className="h-10 w-10 text-destructive"
            aria-hidden="true"
          />
          <p className="text-sm text-muted-foreground">{error}</p>
          <Button variant="outline" onClick={fetchSaved}>
            Try Again
          </Button>
        </div>
      ) : suppliers.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 rounded-2xl border bg-white py-16 shadow-sm">
          <div className="flex h-16 w-16 items-center justify-center rounded-full bg-brand-green-light">
            <Bookmark
              className="h-8 w-8 text-brand-green"
              aria-hidden="true"
            />
          </div>
          <h2 className="text-lg font-semibold text-foreground">
            No saved suppliers yet
          </h2>
          <p className="max-w-sm text-center text-sm text-muted-foreground">
            Browse our directory and save suppliers you are interested in. They
            will appear here for quick access.
          </p>
          <Link href="/suppliers">
            <Button>
              Browse Suppliers
              <ArrowRight className="ml-1 h-4 w-4" />
            </Button>
          </Link>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {suppliers.map((supplier) => (
            <SavedSupplierCard
              key={supplier.id}
              supplier={supplier}
              onRemove={handleRemove}
              removing={removingId === supplier.id}
            />
          ))}
        </div>
      )}
    </div>
  );
}
