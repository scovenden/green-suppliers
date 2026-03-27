"use client";

import * as React from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { X, SlidersHorizontal } from "lucide-react";
import {
  Sheet,
  SheetTrigger,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
} from "@/components/ui/sheet";
import { cn } from "@/lib/utils";
import type { Industry, Country, SdgDto } from "@/lib/types";
import { apiGet } from "@/lib/api-client";

// ---------------------------------------------------------------------------
// Fallback data for when API is unreachable
// ---------------------------------------------------------------------------

// Phase 1: South Africa only
const fallbackCountries: Country[] = [
  { code: "ZA", name: "South Africa", slug: "south-africa", region: "Southern Africa", isActive: true, supplierCount: 15 },
];

// Real industries matching database
const fallbackIndustries: Industry[] = [
  { id: "1", name: "Renewable Energy", slug: "renewable-energy", description: null, parentId: null, sortOrder: 1, isActive: true, supplierCount: 4 },
  { id: "2", name: "Construction", slug: "construction", description: null, parentId: null, sortOrder: 2, isActive: true, supplierCount: 3 },
  { id: "3", name: "Agriculture", slug: "agriculture", description: null, parentId: null, sortOrder: 3, isActive: true, supplierCount: 1 },
  { id: "4", name: "Waste Management", slug: "waste-management", description: null, parentId: null, sortOrder: 4, isActive: true, supplierCount: 5 },
  { id: "5", name: "Water Solutions", slug: "water-solutions", description: null, parentId: null, sortOrder: 5, isActive: true, supplierCount: 2 },
  { id: "6", name: "Manufacturing", slug: "manufacturing", description: null, parentId: null, sortOrder: 6, isActive: true, supplierCount: 8 },
];

const esgLevels = ["bronze", "silver", "gold", "platinum"] as const;

const esgLevelColors: Record<string, string> = {
  bronze: "bg-amber-700",
  silver: "bg-gray-400",
  gold: "bg-amber-500",
  platinum: "bg-green-600",
};

// ---------------------------------------------------------------------------
// Filter content (shared between desktop sidebar and mobile sheet)
// ---------------------------------------------------------------------------

// Abbreviated SDG names for pill labels
const sdgShortNames: Record<number, string> = {
  1: "No Poverty",
  2: "Zero Hunger",
  3: "Good Health",
  4: "Education",
  5: "Gender Equality",
  6: "Clean Water",
  7: "Clean Energy",
  8: "Decent Work",
  9: "Industry & Infra",
  10: "Reduced Inequality",
  11: "Sustainable Cities",
  12: "Responsible Prod.",
  13: "Climate Action",
  14: "Life Below Water",
  15: "Life on Land",
  16: "Peace & Justice",
  17: "Partnerships",
};

interface FilterContentProps {
  countries: Country[];
  industries: Industry[];
  sdgs: SdgDto[];
  selectedCountry: string;
  selectedIndustry: string;
  selectedEsgLevels: string[];
  selectedSdgs: number[];
  verifiedOnly: boolean;
  onCountryChange: (value: string) => void;
  onIndustryChange: (value: string) => void;
  onEsgToggle: (level: string) => void;
  onSdgToggle: (sdgId: number) => void;
  onVerifiedToggle: () => void;
  onClear: () => void;
  hasActiveFilters: boolean;
}

function FilterContent({
  countries,
  industries,
  sdgs,
  selectedCountry,
  selectedIndustry,
  selectedEsgLevels,
  selectedSdgs,
  verifiedOnly,
  onCountryChange,
  onIndustryChange,
  onEsgToggle,
  onSdgToggle,
  onVerifiedToggle,
  onClear,
  hasActiveFilters,
}: FilterContentProps) {
  return (
    <div className="flex flex-col gap-6">
      {/* Country filter */}
      <div className="flex flex-col gap-2">
        <Label className="text-sm font-semibold text-gray-700">Country</Label>
        <select
          value={selectedCountry}
          onChange={(e) => onCountryChange(e.target.value)}
          className="h-9 w-full rounded-lg border border-gray-200 bg-white px-3 text-sm text-gray-700 outline-none transition-colors focus:border-brand-green focus:ring-2 focus:ring-brand-green/20"
        >
          <option value="">All Countries</option>
          {countries.map((c) => (
            <option key={c.code} value={c.code}>
              {c.name} ({c.supplierCount})
            </option>
          ))}
        </select>
      </div>

      {/* Industry filter */}
      <div className="flex flex-col gap-2">
        <Label className="text-sm font-semibold text-gray-700">Industry</Label>
        <select
          value={selectedIndustry}
          onChange={(e) => onIndustryChange(e.target.value)}
          className="h-9 w-full rounded-lg border border-gray-200 bg-white px-3 text-sm text-gray-700 outline-none transition-colors focus:border-brand-green focus:ring-2 focus:ring-brand-green/20"
        >
          <option value="">All Industries</option>
          {industries.map((ind) => (
            <option key={ind.id} value={ind.slug}>
              {ind.name} ({ind.supplierCount})
            </option>
          ))}
        </select>
      </div>

      {/* ESG Level filter */}
      <div className="flex flex-col gap-2">
        <Label className="text-sm font-semibold text-gray-700">ESG Level</Label>
        <div className="flex flex-col gap-2">
          {esgLevels.map((level) => (
            <label
              key={level}
              className="flex cursor-pointer items-center gap-2.5"
            >
              <input
                type="checkbox"
                checked={selectedEsgLevels.includes(level)}
                onChange={() => onEsgToggle(level)}
                className="h-4 w-4 rounded border-gray-300 text-brand-green accent-brand-green focus:ring-brand-green"
              />
              <span className={cn("h-2.5 w-2.5 rounded-full", esgLevelColors[level])} />
              <span className="text-sm capitalize text-gray-700">{level}</span>
            </label>
          ))}
        </div>
      </div>

      {/* SDG Goals filter */}
      {sdgs.length > 0 && (
        <div className="flex flex-col gap-2">
          <Label className="text-sm font-semibold text-gray-700">SDG Goals</Label>
          <div className="flex flex-wrap gap-1.5">
            {sdgs.map((sdg) => {
              const isSelected = selectedSdgs.includes(sdg.id);
              return (
                <button
                  key={sdg.id}
                  type="button"
                  onClick={() => onSdgToggle(sdg.id)}
                  className={cn(
                    "inline-flex items-center gap-1 rounded-full px-2.5 py-1 text-xs font-semibold transition-all duration-200",
                    isSelected
                      ? "text-white shadow-sm ring-2 ring-offset-1"
                      : "text-white/80 opacity-70 hover:opacity-100"
                  )}
                  style={{
                    backgroundColor: sdg.color,
                    ...(isSelected ? { ringColor: sdg.color } : {}),
                  }}
                  title={sdg.name}
                >
                  <span>{sdg.id}</span>
                  <span className="hidden sm:inline">{sdgShortNames[sdg.id] ?? sdg.name.slice(0, 12)}</span>
                </button>
              );
            })}
          </div>
        </div>
      )}

      {/* Verified only */}
      <div className="flex flex-col gap-2">
        <Label className="text-sm font-semibold text-gray-700">Verification</Label>
        <label className="flex cursor-pointer items-center gap-2.5">
          <input
            type="checkbox"
            checked={verifiedOnly}
            onChange={onVerifiedToggle}
            className="h-4 w-4 rounded border-gray-300 text-brand-green accent-brand-green focus:ring-brand-green"
          />
          <span className="text-sm text-gray-700">Verified only</span>
        </label>
      </div>

      {/* Clear filters */}
      {hasActiveFilters && (
        <Button
          variant="outline"
          onClick={onClear}
          className="mt-2 w-full gap-1.5"
        >
          <X className="h-3.5 w-3.5" />
          Clear Filters
        </Button>
      )}
    </div>
  );
}

// ---------------------------------------------------------------------------
// Main FilterSidebar component
// ---------------------------------------------------------------------------

export function FilterSidebar() {
  const router = useRouter();
  const searchParams = useSearchParams();

  const [countries, setCountries] = React.useState<Country[]>(fallbackCountries);
  const [industries, setIndustries] = React.useState<Industry[]>(fallbackIndustries);
  const [sdgs, setSdgs] = React.useState<SdgDto[]>([]);
  const [mobileOpen, setMobileOpen] = React.useState(false);

  // Read current filter state from URL
  const selectedCountry = searchParams.get("countryCode") ?? "";
  const selectedIndustry = searchParams.get("industrySlug") ?? "";
  const selectedEsgLevels = searchParams.get("esgLevel")?.split(",").filter(Boolean) ?? [];
  const selectedSdgs = searchParams.get("sdg")?.split(",").filter(Boolean).map(Number).filter((n) => !isNaN(n)) ?? [];
  const verifiedOnly = searchParams.get("verificationStatus") === "Verified";

  const hasActiveFilters =
    selectedCountry !== "" ||
    selectedIndustry !== "" ||
    selectedEsgLevels.length > 0 ||
    selectedSdgs.length > 0 ||
    verifiedOnly;

  // Fetch countries and industries on mount
  React.useEffect(() => {
    async function loadFilterData() {
      try {
        const apiBase = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api/v1";
        const [countriesRes, industriesRes, sdgsRes] = await Promise.all([
          fetch(`${apiBase}/countries`),
          fetch(`${apiBase}/industries`),
          fetch(`${apiBase}/sdgs`),
        ]);

        if (countriesRes.ok) {
          const cData = await countriesRes.json();
          if (cData.success && cData.data && Array.isArray(cData.data) && cData.data.length > 0) {
            setCountries(cData.data);
          }
        }

        if (industriesRes.ok) {
          const iData = await industriesRes.json();
          if (iData.success && iData.data && Array.isArray(iData.data) && iData.data.length > 0) {
            setIndustries(iData.data);
          }
        }

        if (sdgsRes.ok) {
          const sData = await sdgsRes.json();
          if (sData.success && sData.data && Array.isArray(sData.data) && sData.data.length > 0) {
            setSdgs(sData.data);
          }
        }
      } catch {
        // API unreachable, keep fallback data
      }
    }
    loadFilterData();
  }, []);

  // Helper to update URL search params
  function updateParams(updates: Record<string, string | null>) {
    const params = new URLSearchParams(searchParams.toString());
    for (const [key, value] of Object.entries(updates)) {
      if (value === null || value === "") {
        params.delete(key);
      } else {
        params.set(key, value);
      }
    }
    // Reset to page 1 on filter change
    params.delete("page");
    router.push(`/suppliers?${params.toString()}`);
  }

  function handleCountryChange(value: string) {
    updateParams({ countryCode: value || null });
  }

  function handleIndustryChange(value: string) {
    updateParams({ industrySlug: value || null });
  }

  function handleEsgToggle(level: string) {
    const current = [...selectedEsgLevels];
    const idx = current.indexOf(level);
    if (idx >= 0) {
      current.splice(idx, 1);
    } else {
      current.push(level);
    }
    updateParams({ esgLevel: current.length > 0 ? current.join(",") : null });
  }

  function handleSdgToggle(sdgId: number) {
    const current = [...selectedSdgs];
    const idx = current.indexOf(sdgId);
    if (idx >= 0) {
      current.splice(idx, 1);
    } else {
      current.push(sdgId);
    }
    updateParams({ sdg: current.length > 0 ? current.join(",") : null });
  }

  function handleVerifiedToggle() {
    updateParams({
      verificationStatus: verifiedOnly ? null : "Verified",
    });
  }

  function handleClear() {
    const params = new URLSearchParams();
    const q = searchParams.get("q");
    if (q) params.set("q", q);
    router.push(`/suppliers?${params.toString()}`);
  }

  const filterProps: FilterContentProps = {
    countries,
    industries,
    sdgs,
    selectedCountry,
    selectedIndustry,
    selectedEsgLevels,
    selectedSdgs,
    verifiedOnly,
    onCountryChange: handleCountryChange,
    onIndustryChange: handleIndustryChange,
    onEsgToggle: handleEsgToggle,
    onSdgToggle: handleSdgToggle,
    onVerifiedToggle: handleVerifiedToggle,
    onClear: handleClear,
    hasActiveFilters,
  };

  return (
    <>
      {/* Desktop sidebar */}
      <aside className="hidden lg:block">
        <div className="sticky top-24 w-64">
          <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
            <h2 className="mb-4 text-base font-semibold text-gray-900">
              Filters
            </h2>
            <FilterContent {...filterProps} />
          </div>
        </div>
      </aside>

      {/* Mobile filter button + Sheet */}
      <div className="lg:hidden">
        <Sheet open={mobileOpen} onOpenChange={setMobileOpen}>
          <SheetTrigger
            render={
              <Button variant="outline" className="gap-1.5">
                <SlidersHorizontal className="h-4 w-4" />
                Filters
                {hasActiveFilters && (
                  <span className="flex h-5 w-5 items-center justify-center rounded-full bg-brand-green text-xs font-bold text-white">
                    {(selectedCountry ? 1 : 0) +
                      (selectedIndustry ? 1 : 0) +
                      selectedEsgLevels.length +
                      (verifiedOnly ? 1 : 0)}
                  </span>
                )}
              </Button>
            }
          />
          <SheetContent side="left" className="w-80 overflow-y-auto">
            <SheetHeader>
              <SheetTitle>Filters</SheetTitle>
              <SheetDescription>
                Narrow down your supplier search
              </SheetDescription>
            </SheetHeader>
            <div className="px-4 pb-6">
              <FilterContent {...filterProps} />
            </div>
          </SheetContent>
        </Sheet>
      </div>
    </>
  );
}
