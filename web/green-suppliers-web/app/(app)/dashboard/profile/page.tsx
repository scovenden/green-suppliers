"use client";

import { useCallback, useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiGet, apiPut } from "@/lib/api-client";
import type { SupplierProfile, Industry } from "@/lib/types";
import {
  supplierProfileSchema,
  type SupplierProfileFormData,
} from "@/lib/validators";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Skeleton } from "@/components/ui/skeleton";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Switch } from "@/components/ui/switch";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import {
  Save,
  Globe,
  AlertTriangle,
  CheckCircle2,
  Loader2,
  Eye,
} from "lucide-react";

const EMPLOYEE_COUNTS = [
  "1-10",
  "11-50",
  "51-200",
  "201-500",
  "501-1000",
  "1000+",
];

const BBBEE_LEVELS = [
  "Level 1",
  "Level 2",
  "Level 3",
  "Level 4",
  "Level 5",
  "Level 6",
  "Level 7",
  "Level 8",
  "Non-compliant",
  "Exempt",
];

function ProfileSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-8 w-64" />
      <Skeleton className="h-10 w-full max-w-md" />
      <div className="space-y-4">
        {Array.from({ length: 6 }).map((_, i) => (
          <Skeleton key={i} className="h-12 w-full" />
        ))}
      </div>
    </div>
  );
}

function CompletenessBar({ percent }: { percent: number }) {
  return (
    <div className="flex items-center gap-3">
      <div className="flex-1">
        <div className="h-2 overflow-hidden rounded-full bg-muted">
          <div
            className="h-full rounded-full bg-brand-green transition-all duration-500"
            style={{ width: `${percent}%` }}
          />
        </div>
      </div>
      <span className="text-sm font-semibold text-foreground">{percent}%</span>
    </div>
  );
}

function SliderInput({
  label,
  value,
  onChange,
}: {
  label: string;
  value: number;
  onChange: (val: number) => void;
}) {
  return (
    <div className="space-y-2">
      <div className="flex items-center justify-between">
        <Label className="text-sm">{label}</Label>
        <span className="text-sm font-semibold text-brand-green">{value}%</span>
      </div>
      <div className="relative">
        <input
          type="range"
          min={0}
          max={100}
          step={5}
          value={value}
          onChange={(e) => onChange(Number(e.target.value))}
          className="h-2 w-full cursor-pointer appearance-none rounded-full bg-gradient-to-r from-gray-200 via-green-300 to-green-600 accent-brand-green [&::-webkit-slider-thumb]:h-4 [&::-webkit-slider-thumb]:w-4 [&::-webkit-slider-thumb]:appearance-none [&::-webkit-slider-thumb]:rounded-full [&::-webkit-slider-thumb]:bg-brand-green [&::-webkit-slider-thumb]:shadow-md"
        />
      </div>
    </div>
  );
}

export default function ProfileEditorPage() {
  const { token } = useAuth();
  const [profile, setProfile] = useState<SupplierProfile | null>(null);
  const [industries, setIndustries] = useState<Industry[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [publishing, setPublishing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const form = useForm<SupplierProfileFormData>({
    resolver: zodResolver(supplierProfileSchema),
    defaultValues: {
      tradingName: "",
      description: "",
      shortDescription: "",
      yearFounded: "",
      employeeCount: "",
      bbbeeLevel: "",
      city: "",
      province: "",
      website: "",
      phone: "",
      email: "",
      renewableEnergyPercent: 0,
      wasteRecyclingPercent: 0,
      carbonReporting: false,
      waterManagement: false,
      sustainablePackaging: false,
      industryIds: [],
    },
  });

  const loadProfile = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);

    const [profileRes, industriesRes] = await Promise.all([
      apiGetAuth<SupplierProfile>("/supplier/me/profile", token),
      apiGet<Industry[]>("/industries"),
    ]);

    if (profileRes.success && profileRes.data) {
      const p = profileRes.data;
      setProfile(p);
      form.reset({
        tradingName: p.tradingName ?? "",
        description: p.description ?? "",
        shortDescription: p.shortDescription ?? "",
        yearFounded: p.yearFounded ? String(p.yearFounded) : "",
        employeeCount: p.employeeCount ?? "",
        bbbeeLevel: p.bbbeeLevel ?? "",
        city: p.city ?? "",
        province: p.province ?? "",
        website: p.website ?? "",
        phone: p.phone ?? "",
        email: p.email ?? "",
        renewableEnergyPercent: p.renewableEnergyPercent ?? 0,
        wasteRecyclingPercent: p.wasteRecyclingPercent ?? 0,
        carbonReporting: p.carbonReporting,
        waterManagement: p.waterManagement,
        sustainablePackaging: p.sustainablePackaging,
        industryIds: p.industries.map((ind) => ind.id),
      });
    } else {
      setError(profileRes.error?.message ?? "Failed to load profile");
    }

    if (industriesRes.success && industriesRes.data) {
      setIndustries(industriesRes.data);
    }

    setLoading(false);
  }, [token, form]);

  useEffect(() => {
    loadProfile();
  }, [loadProfile]);

  async function onSubmit(data: SupplierProfileFormData) {
    if (!token) return;
    setSaving(true);

    const payload = {
      ...data,
      yearFounded: data.yearFounded ? Number(data.yearFounded) : null,
    };

    const res = await apiPut<SupplierProfile>(
      "/supplier/me/profile",
      payload,
      token
    );

    if (res.success) {
      toast.success("Profile saved successfully");
      if (res.data) setProfile(res.data);
    } else {
      toast.error(res.error?.message ?? "Failed to save profile");
    }
    setSaving(false);
  }

  async function handlePublish() {
    if (!token) return;
    setPublishing(true);

    const res = await apiPut<SupplierProfile>(
      "/supplier/me/publish",
      {},
      token
    );

    if (res.success) {
      toast.success("Profile published! It is now visible to buyers.");
      if (res.data) setProfile(res.data);
    } else {
      toast.error(res.error?.message ?? "Failed to publish profile");
    }
    setPublishing(false);
  }

  // Calculate a rough client-side completeness percentage
  function computeCompleteness(): number {
    const vals = form.getValues();
    const fields = [
      vals.tradingName,
      vals.description,
      vals.shortDescription,
      vals.city,
      vals.website,
      vals.email,
      vals.phone,
      vals.employeeCount,
      vals.yearFounded,
      (vals.industryIds?.length ?? 0) > 0 ? "x" : "",
    ];
    const filled = fields.filter((f) => f && String(f).trim().length > 0).length;
    return Math.round((filled / fields.length) * 100);
  }

  if (loading) return <ProfileSkeleton />;

  if (error || !profile) {
    return (
      <div className="flex flex-col items-center justify-center gap-4 py-20">
        <AlertTriangle className="h-10 w-10 text-destructive" />
        <p className="text-sm text-muted-foreground">{error ?? "Profile not found"}</p>
        <Button variant="outline" onClick={() => window.location.reload()}>
          Try Again
        </Button>
      </div>
    );
  }

  const completeness = computeCompleteness();

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Edit Profile</h1>
          <p className="text-sm text-muted-foreground">
            Update your company information to improve visibility
          </p>
        </div>
        <div className="flex gap-2">
          {completeness >= 50 && !profile.isPublished && (
            <Button
              variant="outline"
              onClick={handlePublish}
              disabled={publishing}
            >
              {publishing ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Globe className="mr-2 h-4 w-4" />
              )}
              Publish Profile
            </Button>
          )}
          {profile.isPublished && (
            <a
              href={`/suppliers/${profile.slug}`}
              target="_blank"
              rel="noopener noreferrer"
            >
              <Button variant="outline">
                <Eye className="mr-2 h-4 w-4" />
                View Public Profile
              </Button>
            </a>
          )}
        </div>
      </div>

      {/* Completeness bar */}
      <div className="rounded-2xl border bg-white p-4 shadow-sm">
        <p className="mb-2 text-xs font-medium uppercase tracking-wide text-muted-foreground">
          Profile Completeness
        </p>
        <CompletenessBar percent={completeness} />
      </div>

      {/* Read-only info */}
      <div className="rounded-2xl border bg-muted/30 px-4 py-3">
        <p className="text-xs text-muted-foreground">
          <strong>Organization:</strong> {profile.organizationName} &nbsp;|&nbsp;
          <strong>Country:</strong> {profile.countryCode} &nbsp;|&nbsp;
          <strong>ESG Level:</strong> {profile.esgLevel} &nbsp;|&nbsp;
          <strong>Score:</strong> {profile.esgScore}
        </p>
      </div>

      {/* Form */}
      <form onSubmit={form.handleSubmit(onSubmit)}>
        <Tabs defaultValue="company">
          <TabsList variant="line" className="mb-6">
            <TabsTrigger value="company">Company Info</TabsTrigger>
            <TabsTrigger value="location">Location</TabsTrigger>
            <TabsTrigger value="contact">Contact</TabsTrigger>
            <TabsTrigger value="sustainability">Sustainability</TabsTrigger>
            <TabsTrigger value="industries">Industries</TabsTrigger>
          </TabsList>

          {/* Company Info Tab */}
          <TabsContent value="company">
            <div className="rounded-2xl border bg-white p-6 shadow-sm">
              <div className="grid gap-5 sm:grid-cols-2">
                <div className="sm:col-span-2">
                  <Label htmlFor="tradingName">Trading Name *</Label>
                  <Input
                    id="tradingName"
                    {...form.register("tradingName")}
                    className="mt-1"
                  />
                  {form.formState.errors.tradingName && (
                    <p className="mt-1 text-xs text-destructive">
                      {form.formState.errors.tradingName.message}
                    </p>
                  )}
                </div>

                <div className="sm:col-span-2">
                  <Label htmlFor="shortDescription">Short Description</Label>
                  <Input
                    id="shortDescription"
                    {...form.register("shortDescription")}
                    placeholder="A brief tagline (max 300 chars)"
                    className="mt-1"
                  />
                </div>

                <div className="sm:col-span-2">
                  <Label htmlFor="description">Full Description</Label>
                  <Textarea
                    id="description"
                    {...form.register("description")}
                    rows={6}
                    placeholder="Tell buyers about your company, products, and sustainability practices..."
                    className="mt-1"
                  />
                </div>

                <div>
                  <Label htmlFor="yearFounded">Year Founded</Label>
                  <Input
                    id="yearFounded"
                    type="number"
                    {...form.register("yearFounded")}
                    placeholder="e.g. 2015"
                    className="mt-1"
                  />
                </div>

                <div>
                  <Label htmlFor="employeeCount">Employee Count</Label>
                  <Controller
                    name="employeeCount"
                    control={form.control}
                    render={({ field }) => (
                      <Select
                        value={field.value || undefined}
                        onValueChange={field.onChange}
                      >
                        <SelectTrigger className="mt-1">
                          <SelectValue placeholder="Select range" />
                        </SelectTrigger>
                        <SelectContent>
                          {EMPLOYEE_COUNTS.map((c) => (
                            <SelectItem key={c} value={c}>
                              {c}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>

                <div>
                  <Label htmlFor="bbbeeLevel">B-BBEE Level</Label>
                  <Controller
                    name="bbbeeLevel"
                    control={form.control}
                    render={({ field }) => (
                      <Select
                        value={field.value || undefined}
                        onValueChange={field.onChange}
                      >
                        <SelectTrigger className="mt-1">
                          <SelectValue placeholder="Select level" />
                        </SelectTrigger>
                        <SelectContent>
                          {BBBEE_LEVELS.map((l) => (
                            <SelectItem key={l} value={l}>
                              {l}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
              </div>
            </div>
          </TabsContent>

          {/* Location Tab */}
          <TabsContent value="location">
            <div className="rounded-2xl border bg-white p-6 shadow-sm">
              <div className="grid gap-5 sm:grid-cols-2">
                <div>
                  <Label>Country (read-only)</Label>
                  <Input
                    value={profile.countryCode}
                    disabled
                    className="mt-1 bg-muted/50"
                  />
                </div>

                <div>
                  <Label htmlFor="province">Province / State</Label>
                  <Input
                    id="province"
                    {...form.register("province")}
                    placeholder="e.g. Gauteng"
                    className="mt-1"
                  />
                </div>

                <div className="sm:col-span-2">
                  <Label htmlFor="city">City</Label>
                  <Input
                    id="city"
                    {...form.register("city")}
                    placeholder="e.g. Johannesburg"
                    className="mt-1"
                  />
                </div>
              </div>
            </div>
          </TabsContent>

          {/* Contact Tab */}
          <TabsContent value="contact">
            <div className="rounded-2xl border bg-white p-6 shadow-sm">
              <div className="grid gap-5 sm:grid-cols-2">
                <div className="sm:col-span-2">
                  <Label htmlFor="website">Website</Label>
                  <Input
                    id="website"
                    {...form.register("website")}
                    placeholder="https://example.co.za"
                    className="mt-1"
                  />
                  {form.formState.errors.website && (
                    <p className="mt-1 text-xs text-destructive">
                      {form.formState.errors.website.message}
                    </p>
                  )}
                </div>

                <div>
                  <Label htmlFor="phone">Phone</Label>
                  <Input
                    id="phone"
                    {...form.register("phone")}
                    placeholder="+27 11 000 0000"
                    className="mt-1"
                  />
                </div>

                <div>
                  <Label htmlFor="email">Contact Email</Label>
                  <Input
                    id="email"
                    type="email"
                    {...form.register("email")}
                    placeholder="info@company.co.za"
                    className="mt-1"
                  />
                  {form.formState.errors.email && (
                    <p className="mt-1 text-xs text-destructive">
                      {form.formState.errors.email.message}
                    </p>
                  )}
                </div>
              </div>
            </div>
          </TabsContent>

          {/* Sustainability Tab */}
          <TabsContent value="sustainability">
            <div className="rounded-2xl border bg-white p-6 shadow-sm">
              <div className="space-y-6">
                <Controller
                  name="renewableEnergyPercent"
                  control={form.control}
                  render={({ field }) => (
                    <SliderInput
                      label="Renewable Energy Usage"
                      value={field.value ?? 0}
                      onChange={field.onChange}
                    />
                  )}
                />

                <Controller
                  name="wasteRecyclingPercent"
                  control={form.control}
                  render={({ field }) => (
                    <SliderInput
                      label="Waste Recycling Rate"
                      value={field.value ?? 0}
                      onChange={field.onChange}
                    />
                  )}
                />

                <div className="space-y-4 pt-2">
                  <Controller
                    name="carbonReporting"
                    control={form.control}
                    render={({ field }) => (
                      <div className="flex items-center justify-between rounded-xl border p-4">
                        <div>
                          <p className="text-sm font-medium">Carbon Reporting</p>
                          <p className="text-xs text-muted-foreground">
                            Does your organization publish carbon emission reports?
                          </p>
                        </div>
                        <Switch
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </div>
                    )}
                  />

                  <Controller
                    name="waterManagement"
                    control={form.control}
                    render={({ field }) => (
                      <div className="flex items-center justify-between rounded-xl border p-4">
                        <div>
                          <p className="text-sm font-medium">Water Management</p>
                          <p className="text-xs text-muted-foreground">
                            Do you have formal water management practices?
                          </p>
                        </div>
                        <Switch
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </div>
                    )}
                  />

                  <Controller
                    name="sustainablePackaging"
                    control={form.control}
                    render={({ field }) => (
                      <div className="flex items-center justify-between rounded-xl border p-4">
                        <div>
                          <p className="text-sm font-medium">Sustainable Packaging</p>
                          <p className="text-xs text-muted-foreground">
                            Do you use sustainable or recyclable packaging?
                          </p>
                        </div>
                        <Switch
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </div>
                    )}
                  />
                </div>
              </div>
            </div>
          </TabsContent>

          {/* Industries Tab */}
          <TabsContent value="industries">
            <div className="rounded-2xl border bg-white p-6 shadow-sm">
              <p className="mb-4 text-sm text-muted-foreground">
                Select the industries your company operates in. This helps buyers find you.
              </p>
              <Controller
                name="industryIds"
                control={form.control}
                render={({ field }) => (
                  <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                    {industries
                      .filter((ind) => ind.isActive)
                      .map((ind) => {
                        const checked = field.value?.includes(ind.id) ?? false;
                        return (
                          <label
                            key={ind.id}
                            className="flex cursor-pointer items-center gap-3 rounded-xl border p-3 transition-colors hover:bg-muted/30"
                          >
                            <Checkbox
                              checked={checked}
                              onCheckedChange={(val) => {
                                if (val) {
                                  field.onChange([...(field.value ?? []), ind.id]);
                                } else {
                                  field.onChange(
                                    (field.value ?? []).filter(
                                      (id: string) => id !== ind.id
                                    )
                                  );
                                }
                              }}
                            />
                            <span className="text-sm">{ind.name}</span>
                          </label>
                        );
                      })}
                  </div>
                )}
              />
              {industries.length === 0 && (
                <p className="text-sm text-muted-foreground">
                  No industries available.
                </p>
              )}
            </div>
          </TabsContent>
        </Tabs>

        {/* Save button (always visible) */}
        <div className="mt-6 flex items-center gap-3">
          <Button type="submit" disabled={saving} className="min-w-[140px]">
            {saving ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Save className="mr-2 h-4 w-4" />
            )}
            {saving ? "Saving..." : "Save Changes"}
          </Button>
          {form.formState.isSubmitSuccessful && !saving && (
            <span className="flex items-center gap-1 text-sm text-green-600">
              <CheckCircle2 className="h-4 w-4" />
              Saved
            </span>
          )}
        </div>
      </form>
    </div>
  );
}
