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
    <section className="relative overflow-hidden bg-gradient-to-br from-[#0f4c2e] via-brand-green-dark to-brand-emerald">
      {/* SVG blob background shapes */}
      <div className="pointer-events-none absolute inset-0 overflow-hidden">
        {/* Large blob top-right */}
        <svg
          className="absolute -top-20 -right-20 h-[500px] w-[500px] opacity-[0.07]"
          viewBox="0 0 500 500"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M440,320Q430,390,370,430Q310,470,240,460Q170,450,110,400Q50,350,40,270Q30,190,90,130Q150,70,230,50Q310,30,370,80Q430,130,440,200Q450,270,440,320Z"
            fill="white"
          />
        </svg>
        {/* Large blob bottom-left */}
        <svg
          className="absolute -bottom-32 -left-24 h-[600px] w-[600px] opacity-[0.05]"
          viewBox="0 0 600 600"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M500,350Q480,450,390,500Q300,550,200,520Q100,490,60,390Q20,290,60,190Q100,90,200,60Q300,30,400,80Q500,130,510,230Q520,330,500,350Z"
            fill="white"
          />
        </svg>
        {/* Medium blob center-left (parallax-like layered depth) */}
        <svg
          className="absolute top-1/4 left-[10%] h-72 w-72 opacity-[0.04]"
          viewBox="0 0 300 300"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M240,180Q220,240,160,260Q100,280,60,220Q20,160,60,100Q100,40,170,40Q240,40,250,110Q260,180,240,180Z"
            fill="white"
          />
        </svg>
        {/* Small accent blob right-center */}
        <svg
          className="absolute top-1/2 right-[15%] h-40 w-40 opacity-[0.06]"
          viewBox="0 0 200 200"
          xmlns="http://www.w3.org/2000/svg"
        >
          <path
            d="M160,120Q140,170,90,170Q40,170,30,120Q20,70,70,50Q120,30,150,70Q180,110,160,120Z"
            fill="white"
          />
        </svg>
      </div>

      <div className="relative mx-auto max-w-7xl px-4 py-20 sm:px-6 sm:py-24 lg:px-8 lg:py-28">
        <div className="mx-auto max-w-3xl text-center">
          {/* Pill badge */}
          <div className="mb-6 inline-flex items-center rounded-full border border-white/20 bg-white/10 px-4 py-1.5 text-sm font-medium text-green-100 backdrop-blur-sm">
            South Africa&apos;s Green Supplier Directory
          </div>

          {/* Heading */}
          <h1 className="text-4xl font-extrabold tracking-tight text-white sm:text-5xl lg:text-6xl">
            Source Verified Green Suppliers Across South Africa — In Minutes
          </h1>

          {/* Subtitle */}
          <p className="mx-auto mt-6 max-w-2xl text-lg leading-relaxed text-green-100/80">
            Cut weeks of supplier due diligence. Search by industry, ESG level, and verified certification — every supplier independently assessed.
          </p>

          {/* Search bar — enlarged primary CTA */}
          <form
            onSubmit={handleSearch}
            className="mx-auto mt-10 max-w-2xl"
          >
            <div
              ref={searchRef}
              className="relative flex flex-col gap-2 rounded-3xl border border-white/10 bg-white/10 p-2.5 shadow-[0_8px_32px_rgba(0,0,0,0.15)] backdrop-blur-md sm:flex-row"
            >
              {/* Text input with autocomplete */}
              <div className="relative flex-1">
                <Search className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-white/50" />
                <input
                  type="text"
                  placeholder="Search suppliers, services, certifications..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  onFocus={() => setShowSuggestions(true)}
                  className="h-14 w-full rounded-2xl bg-white/10 pl-12 pr-4 text-base text-white placeholder:text-white/50 outline-none transition-colors focus:bg-white/15"
                  autoComplete="off"
                />
                {/* Autocomplete suggestions dropdown */}
                {showSuggestions && filteredSuggestions.length > 0 && (
                  <div className="absolute top-full left-0 right-0 z-50 mt-2 overflow-hidden rounded-2xl border border-white/10 bg-[#0f4c2e]/95 shadow-lg backdrop-blur-xl">
                    <div className="px-3 py-2 text-xs font-medium uppercase tracking-wider text-green-300/70">
                      {debouncedSearch.trim() ? "Suggestions" : "Popular searches"}
                    </div>
                    {filteredSuggestions.map((suggestion) => (
                      <button
                        key={suggestion}
                        type="button"
                        onClick={() => handleSuggestionClick(suggestion)}
                        className="flex w-full items-center gap-2 px-3 py-2.5 text-left text-sm text-green-100 transition-colors hover:bg-white/10"
                      >
                        <Search className="h-3.5 w-3.5 shrink-0 text-green-300/50" />
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
                className="h-14 rounded-2xl bg-white/10 px-4 text-sm text-white outline-none transition-colors focus:bg-white/15 sm:w-48 [&>option]:text-gray-900"
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
                className="h-14 rounded-2xl bg-gradient-to-r from-brand-green to-brand-emerald px-8 text-base font-semibold text-white shadow-lg transition-all duration-300 ease-out hover:from-brand-green-hover hover:to-brand-emerald-hover hover:shadow-xl hover:scale-[1.02] active:scale-[0.98]"
              >
                Find Suppliers
              </button>
            </div>
            <p className="text-white/70 text-sm mt-3">Free to search. No account required.</p>
          </form>

          {/* Popular searches animated pills */}
          <div className="mt-8 flex flex-wrap items-center justify-center gap-2">
            <span className="text-sm text-green-200/60">Popular:</span>
            {quickFilters.map((filter, i) => (
              <button
                key={filter.query}
                onClick={() => handleQuickFilter(filter.query)}
                className="animate-pill-in rounded-full border border-white/15 bg-white/5 px-4 py-1.5 text-sm text-green-100 transition-all duration-300 ease-out hover:border-white/30 hover:bg-white/15 hover:scale-105"
                style={{ animationDelay: `${i * 80}ms` }}
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
