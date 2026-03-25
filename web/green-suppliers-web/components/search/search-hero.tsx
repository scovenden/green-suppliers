"use client";

import * as React from "react";
import { useRouter } from "next/navigation";
import { Search } from "lucide-react";
import { cn } from "@/lib/utils";

const quickFilters = [
  { label: "Solar Energy", query: "solar energy" },
  { label: "Waste Management", query: "waste management" },
  { label: "ISO 14001", query: "ISO 14001" },
  { label: "Sustainable Packaging", query: "sustainable packaging" },
  { label: "Water Solutions", query: "water solutions" },
  { label: "Green Construction", query: "green construction" },
];

const industries = [
  "All Industries",
  "Renewable Energy",
  "Waste Management",
  "Sustainable Agriculture",
  "Green Construction",
  "Eco Packaging",
  "Water Solutions",
  "Carbon Management",
  "Sustainable Transport",
];

export function SearchHero() {
  const router = useRouter();
  const [searchTerm, setSearchTerm] = React.useState("");
  const [selectedIndustry, setSelectedIndustry] = React.useState("All Industries");

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    const params = new URLSearchParams();
    if (searchTerm.trim()) params.set("q", searchTerm.trim());
    if (selectedIndustry !== "All Industries") params.set("industry", selectedIndustry);
    router.push(`/suppliers?${params.toString()}`);
  }

  function handleQuickFilter(query: string) {
    router.push(`/suppliers?q=${encodeURIComponent(query)}`);
  }

  return (
    <section className="relative overflow-hidden bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald">
      {/* Decorative circles */}
      <div className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="absolute -top-24 -right-24 h-96 w-96 rounded-full bg-white/5" />
        <div className="absolute -bottom-32 -left-32 h-[500px] w-[500px] rounded-full bg-white/5" />
        <div className="absolute top-1/3 left-1/4 h-64 w-64 rounded-full bg-white/3" />
      </div>

      <div className="relative mx-auto max-w-7xl px-4 py-20 sm:px-6 sm:py-24 lg:px-8 lg:py-28">
        <div className="mx-auto max-w-3xl text-center">
          {/* Pill badge */}
          <div className="mb-6 inline-flex items-center rounded-full border border-white/20 bg-white/10 px-4 py-1.5 text-sm font-medium text-green-100 backdrop-blur-sm">
            South Africa&apos;s Green Supplier Directory
          </div>

          {/* Heading */}
          <h1 className="text-4xl font-extrabold tracking-tight text-white sm:text-5xl lg:text-6xl" style={{ letterSpacing: "-0.5px" }}>
            Find Verified, ESG-Compliant Suppliers You Can Trust
          </h1>

          {/* Subtitle */}
          <p className="mx-auto mt-6 max-w-2xl text-lg leading-relaxed text-green-100/80">
            Search our curated directory of verified green suppliers across South Africa.
            Filter by industry, certification, and sustainability level.
          </p>

          {/* Search bar */}
          <form
            onSubmit={handleSearch}
            className="mx-auto mt-10 max-w-2xl"
          >
            <div className="flex flex-col gap-2 rounded-2xl border border-white/10 bg-white/10 p-2 backdrop-blur-md sm:flex-row">
              {/* Text input */}
              <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-white/50" />
                <input
                  type="text"
                  placeholder="Search suppliers, services, certifications..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="h-12 w-full rounded-xl bg-white/10 pl-10 pr-4 text-sm text-white placeholder:text-white/50 outline-none transition-colors focus:bg-white/15"
                />
              </div>

              {/* Industry select */}
              <select
                value={selectedIndustry}
                onChange={(e) => setSelectedIndustry(e.target.value)}
                className="h-12 rounded-xl bg-white/10 px-4 text-sm text-white outline-none transition-colors focus:bg-white/15 sm:w-48 [&>option]:text-gray-900"
              >
                {industries.map((ind) => (
                  <option key={ind} value={ind}>
                    {ind}
                  </option>
                ))}
              </select>

              {/* Search button */}
              <button
                type="submit"
                className="h-12 rounded-xl bg-gradient-to-r from-brand-green to-brand-emerald px-6 text-sm font-semibold text-white shadow-lg transition-all hover:from-brand-green-hover hover:to-brand-emerald-hover hover:shadow-xl active:translate-y-px"
              >
                Search
              </button>
            </div>
          </form>

          {/* Quick filter pills */}
          <div className="mt-6 flex flex-wrap items-center justify-center gap-2">
            <span className="text-sm text-green-200/60">Popular:</span>
            {quickFilters.map((filter) => (
              <button
                key={filter.query}
                onClick={() => handleQuickFilter(filter.query)}
                className="rounded-full border border-white/15 bg-white/5 px-3 py-1 text-sm text-green-100 transition-colors hover:border-white/25 hover:bg-white/10"
              >
                {filter.label}
              </button>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
