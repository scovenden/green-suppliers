"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiPost } from "@/lib/api-client";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import type {
  CreateSupplierRequest,
  Industry,
  ServiceTag,
} from "@/lib/types";
import { ArrowLeft, Loader2 } from "lucide-react";
import Link from "next/link";

export default function CreateSupplierPage() {
  const { token } = useAuth();
  const router = useRouter();
  const [industries, setIndustries] = useState<Industry[]>([]);
  const [serviceTags, setServiceTags] = useState<ServiceTag[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});

  // Form state
  const [form, setForm] = useState<CreateSupplierRequest>({
    organizationName: "",
    tradingName: "",
    description: "",
    shortDescription: "",
    yearFounded: undefined,
    employeeCount: "",
    bbbeeLevel: "",
    countryCode: "ZA",
    city: "",
    province: "",
    website: "",
    phone: "",
    email: "",
    renewableEnergyPercent: undefined,
    wasteRecyclingPercent: undefined,
    carbonReporting: false,
    waterManagement: false,
    sustainablePackaging: false,
    industryIds: [],
    serviceTagIds: [],
  });

  useEffect(() => {
    if (!token) return;

    async function fetchTaxonomies() {
      const [indRes, tagRes] = await Promise.all([
        apiGetAuth<Industry[]>("/industries", token!),
        apiGetAuth<ServiceTag[]>("/service-tags", token!),
      ]);
      if (indRes.success && indRes.data) setIndustries(indRes.data);
      if (tagRes.success && tagRes.data) setServiceTags(tagRes.data);
    }

    fetchTaxonomies();
  }, [token]);

  function updateField<K extends keyof CreateSupplierRequest>(
    key: K,
    value: CreateSupplierRequest[K]
  ) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  function toggleArrayItem(
    key: "industryIds" | "serviceTagIds",
    id: string
  ) {
    setForm((prev) => {
      const arr = prev[key];
      return {
        ...prev,
        [key]: arr.includes(id)
          ? arr.filter((v) => v !== id)
          : [...arr, id],
      };
    });
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!token) return;

    setError(null);
    setFieldErrors({});
    setIsSubmitting(true);

    const payload = {
      ...form,
      yearFounded: form.yearFounded || undefined,
      renewableEnergyPercent: form.renewableEnergyPercent ?? undefined,
      wasteRecyclingPercent: form.wasteRecyclingPercent ?? undefined,
    };

    const res = await apiPost("/admin/suppliers", payload, token);

    if (res.success) {
      router.push("/admin/suppliers");
    } else {
      setError(res.error?.message ?? "Failed to create supplier");
      if (res.error?.details) {
        setFieldErrors(res.error.details);
      }
    }
    setIsSubmitting(false);
  }

  function renderFieldError(field: string) {
    const errors = fieldErrors[field];
    if (!errors || errors.length === 0) return null;
    return (
      <p className="text-xs text-destructive mt-1">{errors[0]}</p>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Link href="/admin/suppliers">
          <Button variant="outline" size="icon">
            <ArrowLeft className="h-4 w-4" />
          </Button>
        </Link>
        <div>
          <h1 className="text-2xl font-bold text-foreground">
            Create Supplier
          </h1>
          <p className="text-sm text-muted-foreground">
            Add a new supplier to the directory
          </p>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="space-y-6">
        {error && (
          <div className="rounded-md bg-destructive/10 px-4 py-3 text-sm text-destructive">
            {error}
          </div>
        )}

        {/* Basic Information */}
        <Card>
          <CardHeader>
            <CardTitle>Basic Information</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="organizationName">
                Legal / Organization Name *
              </Label>
              <Input
                id="organizationName"
                value={form.organizationName}
                onChange={(e) =>
                  updateField("organizationName", e.target.value)
                }
                required
              />
              {renderFieldError("OrganizationName")}
            </div>
            <div className="space-y-2">
              <Label htmlFor="tradingName">Trading Name *</Label>
              <Input
                id="tradingName"
                value={form.tradingName}
                onChange={(e) => updateField("tradingName", e.target.value)}
                required
              />
              {renderFieldError("TradingName")}
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label htmlFor="shortDescription">Short Description</Label>
              <Input
                id="shortDescription"
                value={form.shortDescription ?? ""}
                onChange={(e) =>
                  updateField("shortDescription", e.target.value)
                }
                maxLength={200}
                placeholder="Brief one-liner for search results"
              />
            </div>
            <div className="space-y-2 sm:col-span-2">
              <Label htmlFor="description">Full Description</Label>
              <Textarea
                id="description"
                value={form.description ?? ""}
                onChange={(e) => updateField("description", e.target.value)}
                rows={4}
                placeholder="Detailed description of the supplier's products, services, and sustainability practices"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="yearFounded">Year Founded</Label>
              <Input
                id="yearFounded"
                type="number"
                value={form.yearFounded ?? ""}
                onChange={(e) =>
                  updateField(
                    "yearFounded",
                    e.target.value ? parseInt(e.target.value) : undefined
                  )
                }
                min={1800}
                max={new Date().getFullYear()}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="employeeCount">Employee Count</Label>
              <Input
                id="employeeCount"
                value={form.employeeCount ?? ""}
                onChange={(e) =>
                  updateField("employeeCount", e.target.value)
                }
                placeholder="e.g. 50-200"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="bbbeeLevel">B-BBEE Level</Label>
              <Input
                id="bbbeeLevel"
                value={form.bbbeeLevel ?? ""}
                onChange={(e) => updateField("bbbeeLevel", e.target.value)}
                placeholder="e.g. Level 1"
              />
            </div>
          </CardContent>
        </Card>

        {/* Location & Contact */}
        <Card>
          <CardHeader>
            <CardTitle>Location & Contact</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="countryCode">Country Code *</Label>
              <Input
                id="countryCode"
                value={form.countryCode}
                onChange={(e) =>
                  updateField(
                    "countryCode",
                    e.target.value.toUpperCase().slice(0, 2)
                  )
                }
                maxLength={2}
                required
                placeholder="ZA"
              />
              {renderFieldError("CountryCode")}
            </div>
            <div className="space-y-2">
              <Label htmlFor="city">City</Label>
              <Input
                id="city"
                value={form.city ?? ""}
                onChange={(e) => updateField("city", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="province">Province / State</Label>
              <Input
                id="province"
                value={form.province ?? ""}
                onChange={(e) => updateField("province", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="website">Website</Label>
              <Input
                id="website"
                type="url"
                value={form.website ?? ""}
                onChange={(e) => updateField("website", e.target.value)}
                placeholder="https://"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="phone">Phone</Label>
              <Input
                id="phone"
                type="tel"
                value={form.phone ?? ""}
                onChange={(e) => updateField("phone", e.target.value)}
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="supplierEmail">Email</Label>
              <Input
                id="supplierEmail"
                type="email"
                value={form.email ?? ""}
                onChange={(e) => updateField("email", e.target.value)}
              />
            </div>
          </CardContent>
        </Card>

        {/* Sustainability */}
        <Card>
          <CardHeader>
            <CardTitle>Sustainability Attributes</CardTitle>
          </CardHeader>
          <CardContent className="grid gap-4 sm:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="renewableEnergyPercent">
                Renewable Energy (%)
              </Label>
              <Input
                id="renewableEnergyPercent"
                type="number"
                min={0}
                max={100}
                value={form.renewableEnergyPercent ?? ""}
                onChange={(e) =>
                  updateField(
                    "renewableEnergyPercent",
                    e.target.value ? parseInt(e.target.value) : undefined
                  )
                }
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="wasteRecyclingPercent">
                Waste Recycling (%)
              </Label>
              <Input
                id="wasteRecyclingPercent"
                type="number"
                min={0}
                max={100}
                value={form.wasteRecyclingPercent ?? ""}
                onChange={(e) =>
                  updateField(
                    "wasteRecyclingPercent",
                    e.target.value ? parseInt(e.target.value) : undefined
                  )
                }
              />
            </div>
            <div className="flex items-center gap-6 sm:col-span-2">
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={form.carbonReporting}
                  onChange={(e) =>
                    updateField("carbonReporting", e.target.checked)
                  }
                  className="h-4 w-4 rounded border-input accent-brand-green"
                />
                Carbon Reporting
              </label>
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={form.waterManagement}
                  onChange={(e) =>
                    updateField("waterManagement", e.target.checked)
                  }
                  className="h-4 w-4 rounded border-input accent-brand-green"
                />
                Water Management
              </label>
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={form.sustainablePackaging}
                  onChange={(e) =>
                    updateField("sustainablePackaging", e.target.checked)
                  }
                  className="h-4 w-4 rounded border-input accent-brand-green"
                />
                Sustainable Packaging
              </label>
            </div>
          </CardContent>
        </Card>

        {/* Industries */}
        <Card>
          <CardHeader>
            <CardTitle>Industries</CardTitle>
          </CardHeader>
          <CardContent>
            {industries.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                No industries available. Add them in the Taxonomy section first.
              </p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {industries
                  .filter((ind) => ind.isActive)
                  .map((ind) => {
                    const selected = form.industryIds.includes(ind.id);
                    return (
                      <button
                        key={ind.id}
                        type="button"
                        onClick={() => toggleArrayItem("industryIds", ind.id)}
                        className={`rounded-full border px-3 py-1 text-sm transition-colors ${
                          selected
                            ? "border-brand-green bg-brand-green text-white"
                            : "border-input bg-white text-foreground hover:bg-muted"
                        }`}
                      >
                        {ind.name}
                      </button>
                    );
                  })}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Service Tags */}
        <Card>
          <CardHeader>
            <CardTitle>Service Tags</CardTitle>
          </CardHeader>
          <CardContent>
            {serviceTags.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                No service tags available. Add them in the Taxonomy section
                first.
              </p>
            ) : (
              <div className="flex flex-wrap gap-2">
                {serviceTags
                  .filter((tag) => tag.isActive)
                  .map((tag) => {
                    const selected = form.serviceTagIds.includes(tag.id);
                    return (
                      <button
                        key={tag.id}
                        type="button"
                        onClick={() =>
                          toggleArrayItem("serviceTagIds", tag.id)
                        }
                        className={`rounded-full border px-3 py-1 text-sm transition-colors ${
                          selected
                            ? "border-brand-emerald bg-brand-emerald text-white"
                            : "border-input bg-white text-foreground hover:bg-muted"
                        }`}
                      >
                        {tag.name}
                      </button>
                    );
                  })}
              </div>
            )}
          </CardContent>
        </Card>

        {/* Submit */}
        <div className="flex justify-end gap-3">
          <Link href="/admin/suppliers">
            <Button type="button" variant="outline">
              Cancel
            </Button>
          </Link>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Creating...
              </>
            ) : (
              "Create Supplier"
            )}
          </Button>
        </div>
      </form>
    </div>
  );
}
