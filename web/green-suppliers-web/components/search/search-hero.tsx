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
  { label: "Cape Town", query: "Cape Town" },
  { label: "Johannesburg", query: "Johannesburg" },
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

const popularSuggestions = [
  "Solar panels",
  "ISO 14001 certified",
  "Recycled packaging",
  "Water treatment",
  "Carbon offsetting",
  "Green building materials",
  "Organic farming",
  "EV fleet management",
];

function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = React.useState(value);
  React.useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay);
    return () => clearTimeout(timer);
  }, [value, delay]);
  return debouncedValue;
}

export function SearchHero() {
  const router = useRouter();
  const [searchTerm, setSearchTerm] = React.useState("");
  const [selectedIndustry, setSelectedIndustry] = React.useState("All Industries");
  const [showSuggestions, setShowSuggestions] = React.useState(false);
  const debouncedSearch = useDebounce(searchTerm, 300);
  const searchRef = React.useRef<HTMLDivElement>(null);

  // Filter suggestions based on debounced input
  const filteredSuggestions = React.useMemo(() => {
    if (!debouncedSearch.trim()) return popularSuggestions;
    const lq = debouncedSearch.toLowerCase();
    return popularSuggestions.filter((s) =>
      s.toLowerCase().includes(lq)
    );
  }, [debouncedSearch]);

  // Close suggestions on outside click
  React.useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (searchRef.current && !searchRef.current.contains(e.target as Node)) {
        setShowSuggestions(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  function handleSearch(e: React.FormEvent) {
    e.preventDefault();
    setShowSuggestions(false);
    const params = new URLSearchParams();
    if (searchTerm.trim()) params.set("q", searchTerm.trim());
    if (selectedIndustry !== "All Industries") params.set("industry", selectedIndustry);
    router.push(`/suppliers?${params.toString()}`);
  }

  function handleQuickFilter(query: string) {
    router.push(`/suppliers?q=${encodeURIComponent(query)}`);
  }

  function handleSuggestionClick(suggestion: string) {
    setSearchTerm(suggestion);
    setShowSuggestions(false);
    router.push(`/suppliers?q=${encodeURIComponent(suggestion)}`);
  }

  return (
    <section className="relative overflow-hidden bg-[#0F172A]">
      {/* Background texture image — nature/sustainability at very low opacity */}
      <div
        className="pointer-events-none absolute inset-0 bg-cover bg-center opacity-[0.06]"
        style={{
          backgroundImage:
            "url('https://images.unsplash.com/photo-1441974231531-c6227db76b6e?w=1920&q=80')",
        }}
      />

      {/* Subtle dot grid pattern overlay */}
      <div
        className="pointer-events-none absolute inset-0 opacity-[0.04]"
        style={{
          backgroundImage:
            "radial-gradient(circle, rgba(255,255,255,0.8) 1px, transparent 1px)",
          backgroundSize: "24px 24px",
        }}
      />

      {/* Green gradient accent — right side glow for brand identity */}
      <div className="pointer-events-none absolute -right-32 top-0 h-full w-1/2 bg-gradient-to-l from-brand-green/[0.08] via-brand-emerald/[0.04] to-transparent" />
      <div className="pointer-events-none absolute -left-32 bottom-0 h-1/2 w-1/3 bg-gradient-to-tr from-brand-green/[0.06] to-transparent" />

      {/* Floating geometric shapes for depth */}
      <div className="pointer-events-none absolute inset-0 overflow-hidden">
        {/* Large hexagon top-right */}
        <svg
          className="absolute -top-16 -right-16 h-[400px] w-[400px] opacity-[0.03]"
          viewBox="0 0 400 400"
          xmlns="http://www.w3.org/2000/svg"
        >
          <polygon
            points="200,20 370,110 370,290 200,380 30,290 30,110"
            fill="none"
            stroke="#16A34A"
            strokeWidth="1.5"
          />
        </svg>
        {/* Medium circle center-left */}
        <svg
          className="absolute top-1/4 left-[8%] h-64 w-64 opacity-[0.03]"
          viewBox="0 0 256 256"
          xmlns="http://www.w3.org/2000/svg"
        >
          <circle cx="128" cy="128" r="120" fill="none" stroke="#16A34A" strokeWidth="1" />
          <circle cx="128" cy="128" r="80" fill="none" stroke="#16A34A" strokeWidth="0.5" />
        </svg>
        {/* Small hexagon bottom-left */}
        <svg
          className="absolute bottom-20 left-[15%] h-32 w-32 opacity-[0.04]"
          viewBox="0 0 128 128"
          xmlns="http://www.w3.org/2000/svg"
        >
          <polygon
            points="64,8 120,36 120,92 64,120 8,92 8,36"
            fill="none"
            stroke="white"
            strokeWidth="0.8"
          />
        </svg>
        {/* Circle top-center */}
        <svg
          className="absolute top-12 left-1/2 h-48 w-48 -translate-x-1/2 opacity-[0.02]"
          viewBox="0 0 192 192"
          xmlns="http://www.w3.org/2000/svg"
        >
          <circle cx="96" cy="96" r="90" fill="none" stroke="white" strokeWidth="0.8" />
        </svg>
        {/* Small diamond right-center */}
        <svg
          className="absolute top-1/2 right-[12%] h-24 w-24 opacity-[0.04] -translate-y-1/2"
          viewBox="0 0 96 96"
          xmlns="http://www.w3.org/2000/svg"
        >
          <polygon
            points="48,4 92,48 48,92 4,48"
            fill="none"
            stroke="#16A34A"
            strokeWidth="0.8"
          />
        </svg>
      </div>

      <div className="relative mx-auto max-w-7xl px-4 py-20 sm:px-6 sm:py-24 lg:px-8 lg:py-28">
        <div className="mx-auto max-w-3xl text-center">
          {/* Pill badge — solid green on dark for strong brand identity */}
          <div className="mb-6 inline-flex items-center rounded-full bg-brand-green px-4 py-1.5 text-sm font-semibold text-white shadow-lg shadow-brand-green/20">
            <span className="mr-2 inline-block h-2 w-2 rounded-full bg-white/80 animate-pulse" />
            South Africa&apos;s Green Supplier Directory
          </div>

          {/* Heading — white on dark for maximum contrast */}
          <h1 className="text-4xl font-extrabold tracking-tight text-white sm:text-5xl lg:text-6xl">
            Source Verified Green Suppliers Across South Africa{" "}
            <span className="bg-gradient-to-r from-brand-green to-emerald-400 bg-clip-text text-transparent">
              In Minutes
            </span>
          </h1>

          {/* Subtitle — slate gray for clear hierarchy without green tint */}
          <p className="mx-auto mt-6 max-w-2xl text-lg leading-relaxed text-slate-400">
            Cut weeks of supplier due diligence. Search by industry, ESG level, and verified certification — every supplier independently assessed.
          </p>

          {/* Search bar — glassmorphism on dark background */}
          <form
            onSubmit={handleSearch}
            className="mx-auto mt-10 max-w-2xl"
          >
            {/* Green glow behind search bar */}
            <div className="relative">
              <div className="absolute -inset-4 rounded-[2rem] bg-brand-green/[0.07] blur-2xl" />

              <div
                ref={searchRef}
                className="relative flex flex-col gap-2 rounded-3xl border border-white/[0.12] bg-white/[0.08] p-2.5 shadow-[0_8px_32px_rgba(0,0,0,0.3)] backdrop-blur-xl sm:flex-row"
              >
                {/* Text input with autocomplete */}
                <div className="relative flex-1">
                  <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-slate-500" />
                  <input
                    type="text"
                    placeholder="Search suppliers, services, certifications..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    onFocus={() => { if (searchTerm.trim().length > 0) setShowSuggestions(true); }}
                    className="h-14 w-full rounded-2xl bg-white/[0.08] pl-12 pr-4 text-base text-white placeholder:text-slate-500 outline-none transition-colors focus:bg-white/[0.12]"
                    autoComplete="off"
                  />
                  {/* Autocomplete suggestions dropdown */}
                  {showSuggestions && filteredSuggestions.length > 0 && (
                    <div className="absolute top-full left-0 right-0 z-50 mt-2 overflow-hidden rounded-2xl border border-white/[0.1] bg-[#0F172A]/95 shadow-lg backdrop-blur-xl">
                      <div className="px-3 py-2 text-xs font-medium uppercase tracking-wider text-slate-500">
                        {debouncedSearch.trim() ? "Suggestions" : "Popular searches"}
                      </div>
                      {filteredSuggestions.map((suggestion) => (
                        <button
                          key={suggestion}
                          type="button"
                          onClick={() => handleSuggestionClick(suggestion)}
                          className="flex w-full items-center gap-2 px-3 py-2.5 text-left text-sm text-slate-300 transition-colors hover:bg-white/[0.08] hover:text-white"
                        >
                          <Search className="h-3.5 w-3.5 shrink-0 text-slate-600" />
                          {suggestion}
                        </button>
                      ))}
                    </div>
                  )}
                </div>

                {/* Industry select */}
                <select
                  value={selectedIndustry}
                  onChange={(e) => setSelectedIndustry(e.target.value)}
                  className="h-14 rounded-2xl bg-white/[0.08] px-4 text-sm text-white outline-none transition-colors focus:bg-white/[0.12] sm:w-48 [&>option]:text-gray-900"
                >
                  {industries.map((ind) => (
                    <option key={ind} value={ind}>
                      {ind}
                    </option>
                  ))}
                </select>

                {/* Search button — green gradient CTA */}
                <button
                  type="submit"
                  className="h-14 rounded-2xl bg-gradient-to-r from-brand-green to-brand-emerald px-8 text-base font-semibold text-white shadow-lg shadow-brand-green/25 transition-all duration-300 ease-out hover:from-brand-green-hover hover:to-brand-emerald-hover hover:shadow-xl hover:shadow-brand-green/30 hover:scale-[1.02] active:scale-[0.98]"
                >
                  Find Suppliers
                </button>
              </div>
            </div>
            <p className="text-slate-500 text-sm mt-3">Free to search. No account required.</p>
          </form>

          {/* Quick filter pills — ghost style with green hover */}
          <div className="mt-8 flex flex-wrap items-center justify-center gap-2">
            <span className="text-sm text-slate-500">Popular:</span>
            {quickFilters.map((filter, i) => (
              <button
                key={filter.query}
                onClick={() => handleQuickFilter(filter.query)}
                className="animate-pill-in rounded-full border border-white/[0.15] px-4 py-1.5 text-sm text-white/60 transition-all duration-300 ease-out hover:border-brand-green hover:text-brand-green hover:bg-brand-green/[0.08] hover:scale-105"
                style={{ animationDelay: `${i * 80}ms` }}
              >
                {filter.label}
              </button>
            ))}
          </div>
        </div>
      </div>

      {/* Diagonal clip / wave transition to white content below */}
      <div className="relative h-20 sm:h-24">
        <svg
          className="absolute bottom-0 left-0 w-full"
          viewBox="0 0 1440 96"
          preserveAspectRatio="none"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M0,64 C360,96 720,0 1080,48 C1260,72 1380,88 1440,96 L1440,96 L0,96 Z"
            fill="white"
          />
        </svg>
      </div>
    </section>
  );
}
