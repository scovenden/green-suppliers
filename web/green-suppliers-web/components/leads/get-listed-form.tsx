"use client";

import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { getListedSchema, type GetListedFormData } from "@/lib/validators";
import { apiGet, apiPost } from "@/lib/api-client";
import type { Industry } from "@/lib/types";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import {
  CheckCircle,
  AlertCircle,
  Send,
  Loader2,
  Building2,
} from "lucide-react";

// ---------------------------------------------------------------------------
// Country options
// ---------------------------------------------------------------------------

const countries = [
  { code: "ZA", name: "South Africa" },
  { code: "KE", name: "Kenya" },
  { code: "NG", name: "Nigeria" },
  { code: "GH", name: "Ghana" },
  { code: "EG", name: "Egypt" },
  { code: "MA", name: "Morocco" },
  { code: "RW", name: "Rwanda" },
  { code: "TZ", name: "Tanzania" },
  { code: "UG", name: "Uganda" },
  { code: "BW", name: "Botswana" },
];

// ---------------------------------------------------------------------------
// Component
// ---------------------------------------------------------------------------

export function GetListedForm() {
  const [status, setStatus] = React.useState<
    "idle" | "submitting" | "success" | "error"
  >("idle");
  const [errorMessage, setErrorMessage] = React.useState("");
  const [industries, setIndustries] = React.useState<Industry[]>([]);
  const [selectedIndustries, setSelectedIndustries] = React.useState<string[]>(
    []
  );

  // Fetch industries on mount
  React.useEffect(() => {
    async function fetchIndustries() {
      try {
        const res = await apiGet<Industry[]>("/industries");
        if (res.success && res.data) {
          setIndustries(res.data);
          return;
        }
      } catch {
        // API unreachable
      }
      // Fallback industries
      setIndustries([
        {
          id: "1",
          name: "Renewable Energy",
          slug: "renewable-energy",
          description: null,
          parentId: null,
          sortOrder: 1,
          isActive: true,
          supplierCount: 0,
        },
        {
          id: "2",
          name: "Waste Management",
          slug: "waste-management",
          description: null,
          parentId: null,
          sortOrder: 2,
          isActive: true,
          supplierCount: 0,
        },
        {
          id: "3",
          name: "Sustainable Agriculture",
          slug: "sustainable-agriculture",
          description: null,
          parentId: null,
          sortOrder: 3,
          isActive: true,
          supplierCount: 0,
        },
        {
          id: "4",
          name: "Green Construction",
          slug: "green-construction",
          description: null,
          parentId: null,
          sortOrder: 4,
          isActive: true,
          supplierCount: 0,
        },
        {
          id: "5",
          name: "Eco Packaging",
          slug: "eco-packaging",
          description: null,
          parentId: null,
          sortOrder: 5,
          isActive: true,
          supplierCount: 0,
        },
        {
          id: "6",
          name: "Water Solutions",
          slug: "water-solutions",
          description: null,
          parentId: null,
          sortOrder: 6,
          isActive: true,
          supplierCount: 0,
        },
        {
          id: "7",
          name: "Carbon Management",
          slug: "carbon-management",
          description: null,
          parentId: null,
          sortOrder: 7,
          isActive: true,
          supplierCount: 0,
        },
        {
          id: "8",
          name: "Sustainable Transport",
          slug: "sustainable-transport",
          description: null,
          parentId: null,
          sortOrder: 8,
          isActive: true,
          supplierCount: 0,
        },
      ]);
    }
    fetchIndustries();
  }, []);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<GetListedFormData>({
    resolver: zodResolver(getListedSchema),
    defaultValues: {
      companyName: "",
      contactName: "",
      contactEmail: "",
      contactPhone: "",
      website: "",
      country: "",
      city: "",
      description: "",
      certifications: "",
    },
  });

  function toggleIndustry(id: string) {
    setSelectedIndustries((prev) =>
      prev.includes(id) ? prev.filter((i) => i !== id) : [...prev, id]
    );
  }

  async function onSubmit(data: GetListedFormData) {
    setStatus("submitting");
    setErrorMessage("");

    try {
      const res = await apiPost("/get-listed", {
        ...data,
        industryIds: selectedIndustries,
      });

      if (res.success) {
        setStatus("success");
        reset();
        setSelectedIndustries([]);
      } else {
        setStatus("error");
        setErrorMessage(
          res.error?.message ?? "Something went wrong. Please try again."
        );
      }
    } catch {
      setStatus("error");
      setErrorMessage(
        "Unable to submit your application right now. Please try again later."
      );
    }
  }

  // -------------------------------------------------------------------------
  // Success state
  // -------------------------------------------------------------------------

  if (status === "success") {
    return (
      <div className="flex flex-col items-center gap-4 rounded-2xl border border-green-200 bg-green-50 p-10 text-center">
        <div className="flex h-14 w-14 items-center justify-center rounded-full bg-brand-green/10">
          <CheckCircle className="h-7 w-7 text-brand-green" />
        </div>
        <h3 className="text-xl font-semibold text-gray-900">
          Application Received!
        </h3>
        <p className="max-w-md text-sm leading-relaxed text-gray-600">
          Thank you for applying to be listed on Green Suppliers. Our team
          will review your submission and be in touch within 2 business days.
        </p>
        <Button
          variant="outline"
          onClick={() => setStatus("idle")}
          className="mt-2"
        >
          Submit another application
        </Button>
      </div>
    );
  }

  // -------------------------------------------------------------------------
  // Form
  // -------------------------------------------------------------------------

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="flex flex-col gap-6"
    >
      {status === "error" && errorMessage && (
        <div className="flex items-start gap-2 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          <AlertCircle className="mt-0.5 h-4 w-4 shrink-0" />
          {errorMessage}
        </div>
      )}

      {/* Company details section */}
      <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
        <h3 className="flex items-center gap-2 text-base font-semibold text-gray-900">
          <Building2 className="h-5 w-5 text-brand-green" />
          Company Details
        </h3>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          {/* Company Name */}
          <div className="flex flex-col gap-1.5 sm:col-span-2">
            <Label htmlFor="companyName">
              Company Name <span className="text-red-500">*</span>
            </Label>
            <Input
              id="companyName"
              placeholder="Your company or trading name"
              aria-invalid={!!errors.companyName}
              {...register("companyName")}
            />
            {errors.companyName && (
              <p className="text-xs text-red-500">
                {errors.companyName.message}
              </p>
            )}
          </div>

          {/* Website */}
          <div className="flex flex-col gap-1.5 sm:col-span-2">
            <Label htmlFor="website">Website</Label>
            <Input
              id="website"
              type="url"
              placeholder="https://www.yourcompany.co.za"
              {...register("website")}
            />
            {errors.website && (
              <p className="text-xs text-red-500">{errors.website.message}</p>
            )}
          </div>

          {/* Country */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="country">
              Country <span className="text-red-500">*</span>
            </Label>
            <select
              id="country"
              className="h-9 w-full rounded-md border border-input bg-transparent px-3 py-1 text-sm shadow-xs transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
              aria-invalid={!!errors.country}
              {...register("country")}
            >
              <option value="">Select a country</option>
              {countries.map((c) => (
                <option key={c.code} value={c.code}>
                  {c.name}
                </option>
              ))}
            </select>
            {errors.country && (
              <p className="text-xs text-red-500">{errors.country.message}</p>
            )}
          </div>

          {/* City */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="city">City</Label>
            <Input
              id="city"
              placeholder="e.g. Cape Town, Johannesburg"
              {...register("city")}
            />
          </div>

          {/* Description */}
          <div className="flex flex-col gap-1.5 sm:col-span-2">
            <Label htmlFor="description">
              What does your company do?{" "}
              <span className="text-red-500">*</span>
            </Label>
            <Textarea
              id="description"
              placeholder="Briefly describe your products, services, and sustainability practices (max 500 characters)"
              rows={4}
              maxLength={500}
              aria-invalid={!!errors.description}
              {...register("description")}
            />
            {errors.description && (
              <p className="text-xs text-red-500">
                {errors.description.message}
              </p>
            )}
          </div>

          {/* Certifications */}
          <div className="flex flex-col gap-1.5 sm:col-span-2">
            <Label htmlFor="certifications">
              Certifications / Accreditations
            </Label>
            <Input
              id="certifications"
              placeholder="e.g. ISO 14001, B-Corp, Green Building Council"
              {...register("certifications")}
            />
            <p className="text-xs text-gray-400">
              List any relevant environmental or sustainability certifications
            </p>
          </div>
        </div>
      </div>

      {/* Industries section */}
      <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
        <h3 className="text-base font-semibold text-gray-900">
          Industries
        </h3>
        <p className="mt-1 text-sm text-gray-500">
          Select all industries that apply to your business
        </p>
        <div className="mt-4 grid gap-2 sm:grid-cols-2">
          {industries.map((industry) => (
            <label
              key={industry.id}
              className="group flex cursor-pointer items-center gap-3 rounded-xl border border-gray-100 px-4 py-3 transition-all hover:border-brand-green/30 hover:bg-brand-green-light/50 has-[:checked]:border-brand-green/40 has-[:checked]:bg-brand-green-light"
            >
              <input
                type="checkbox"
                checked={selectedIndustries.includes(industry.id)}
                onChange={() => toggleIndustry(industry.id)}
                className="h-4 w-4 rounded border-gray-300 text-brand-green focus:ring-brand-green"
              />
              <span className="text-sm font-medium text-gray-700 group-has-[:checked]:text-brand-green-dark">
                {industry.name}
              </span>
            </label>
          ))}
        </div>
      </div>

      {/* Contact details section */}
      <div className="rounded-2xl border border-gray-100 bg-white p-6 shadow-sm">
        <h3 className="text-base font-semibold text-gray-900">
          Contact Details
        </h3>
        <div className="mt-4 grid gap-4 sm:grid-cols-2">
          {/* Contact Name */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="contactName">
              Contact Name <span className="text-red-500">*</span>
            </Label>
            <Input
              id="contactName"
              placeholder="Your full name"
              aria-invalid={!!errors.contactName}
              {...register("contactName")}
            />
            {errors.contactName && (
              <p className="text-xs text-red-500">
                {errors.contactName.message}
              </p>
            )}
          </div>

          {/* Phone */}
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="contactPhone">Phone</Label>
            <Input
              id="contactPhone"
              type="tel"
              placeholder="+27 12 345 6789"
              {...register("contactPhone")}
            />
          </div>

          {/* Email */}
          <div className="flex flex-col gap-1.5 sm:col-span-2">
            <Label htmlFor="contactEmail">
              Email <span className="text-red-500">*</span>
            </Label>
            <Input
              id="contactEmail"
              type="email"
              placeholder="you@company.co.za"
              aria-invalid={!!errors.contactEmail}
              {...register("contactEmail")}
            />
            {errors.contactEmail && (
              <p className="text-xs text-red-500">
                {errors.contactEmail.message}
              </p>
            )}
          </div>
        </div>
      </div>

      {/* Submit */}
      <Button
        type="submit"
        disabled={status === "submitting"}
        className="h-12 w-full gap-2 rounded-xl bg-brand-green text-base font-semibold text-white shadow-md transition-colors hover:bg-brand-green-hover"
      >
        {status === "submitting" ? (
          <>
            <Loader2 className="h-5 w-5 animate-spin" />
            Submitting...
          </>
        ) : (
          <>
            <Send className="h-5 w-5" />
            Submit Application
          </>
        )}
      </Button>
    </form>
  );
}
