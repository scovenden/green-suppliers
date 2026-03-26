"use client";

import { useCallback, useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { toast } from "sonner";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth, apiGet, apiPost, apiPostMultipart } from "@/lib/api-client";
import type { CertificationDto, CertificationType } from "@/lib/types";
import {
  submitCertificationSchema,
  type SubmitCertificationFormData,
} from "@/lib/validators";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Plus,
  ShieldCheck,
  Clock,
  XCircle,
  AlertTriangle,
  Upload,
  FileText,
  Loader2,
  CalendarDays,
} from "lucide-react";

function CertStatusBadge({ status }: { status: string }) {
  const lower = status.toLowerCase();
  const config: Record<string, { bg: string; text: string; icon: React.ReactNode; label: string }> = {
    accepted: {
      bg: "bg-green-100",
      text: "text-green-700",
      icon: <ShieldCheck className="h-3.5 w-3.5" />,
      label: "Accepted",
    },
    pending: {
      bg: "bg-yellow-100",
      text: "text-yellow-700",
      icon: <Clock className="h-3.5 w-3.5" />,
      label: "Pending",
    },
    rejected: {
      bg: "bg-red-100",
      text: "text-red-700",
      icon: <XCircle className="h-3.5 w-3.5" />,
      label: "Rejected",
    },
    expired: {
      bg: "bg-gray-100",
      text: "text-gray-600",
      icon: <AlertTriangle className="h-3.5 w-3.5" />,
      label: "Expired",
    },
  };
  const c = config[lower] ?? { bg: "bg-gray-100", text: "text-gray-600", icon: null, label: status };

  return (
    <span className={`inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ${c.bg} ${c.text}`}>
      {c.icon}
      {c.label}
    </span>
  );
}

function CertificationCard({ cert }: { cert: CertificationDto }) {
  const expiryDate = cert.expiresAt ? new Date(cert.expiresAt) : null;
  const isExpiringSoon =
    expiryDate &&
    expiryDate > new Date() &&
    expiryDate.getTime() - Date.now() < 30 * 24 * 60 * 60 * 1000;

  return (
    <div className="rounded-2xl border bg-white p-5 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex items-start justify-between gap-3">
        <div className="flex items-start gap-3">
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-brand-green/10">
            <FileText className="h-5 w-5 text-brand-green" />
          </div>
          <div>
            <h3 className="text-sm font-semibold text-foreground">
              {cert.certTypeName}
            </h3>
            {cert.certificateNumber && (
              <p className="text-xs text-muted-foreground">
                #{cert.certificateNumber}
              </p>
            )}
          </div>
        </div>
        <CertStatusBadge status={cert.status} />
      </div>

      <div className="mt-4 flex flex-wrap gap-4 text-xs text-muted-foreground">
        {cert.issuedAt && (
          <div className="flex items-center gap-1">
            <CalendarDays className="h-3.5 w-3.5" />
            Issued: {new Date(cert.issuedAt).toLocaleDateString()}
          </div>
        )}
        {expiryDate && (
          <div
            className={`flex items-center gap-1 ${
              isExpiringSoon ? "font-semibold text-yellow-600" : ""
            }`}
          >
            <CalendarDays className="h-3.5 w-3.5" />
            Expires: {expiryDate.toLocaleDateString()}
            {isExpiringSoon && " (soon)"}
          </div>
        )}
      </div>

      {cert.notes && (
        <p className="mt-3 rounded-lg bg-muted/30 p-2 text-xs text-muted-foreground">
          {cert.notes}
        </p>
      )}
    </div>
  );
}

function CertificationsSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-9 w-40" />
      </div>
      <div className="grid gap-4 sm:grid-cols-2">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-36 rounded-2xl" />
        ))}
      </div>
    </div>
  );
}

export default function CertificationsPage() {
  const { token } = useAuth();
  const [certs, setCerts] = useState<CertificationDto[]>([]);
  const [certTypes, setCertTypes] = useState<CertificationType[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);

  const form = useForm<SubmitCertificationFormData>({
    resolver: zodResolver(submitCertificationSchema),
    defaultValues: {
      certificationTypeId: "",
      certificateNumber: "",
      issuedAt: "",
      expiresAt: "",
    },
  });

  const loadCertifications = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);

    const [certsRes, typesRes] = await Promise.all([
      apiGetAuth<CertificationDto[]>("/supplier/me/certifications", token),
      apiGet<CertificationType[]>("/certification-types"),
    ]);

    if (certsRes.success && certsRes.data) {
      setCerts(certsRes.data);
    } else {
      setError(certsRes.error?.message ?? "Failed to load certifications");
    }

    if (typesRes.success && typesRes.data) {
      setCertTypes(typesRes.data);
    }

    setLoading(false);
  }, [token]);

  useEffect(() => {
    loadCertifications();
  }, [loadCertifications]);

  async function onSubmit(data: SubmitCertificationFormData) {
    if (!token) return;
    setSubmitting(true);

    // First, submit the certification data
    const payload = {
      certificationTypeId: data.certificationTypeId,
      certificateNumber: data.certificateNumber || undefined,
      issuedAt: data.issuedAt || undefined,
      expiresAt: data.expiresAt || undefined,
    };

    const res = await apiPost<CertificationDto>(
      "/supplier/me/certifications",
      payload,
      token
    );

    if (res.success && res.data) {
      // If a file was selected, upload it
      if (selectedFile) {
        const formData = new FormData();
        formData.append("file", selectedFile);
        formData.append("entityType", "certification");
        formData.append("entityId", res.data.id);

        await apiPostMultipart("/supplier/me/documents", formData, token);
      }

      toast.success("Certification submitted for review");
      setDialogOpen(false);
      form.reset();
      setSelectedFile(null);
      await loadCertifications();
    } else {
      toast.error(res.error?.message ?? "Failed to submit certification");
    }

    setSubmitting(false);
  }

  if (loading) return <CertificationsSkeleton />;

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center gap-4 py-20">
        <AlertTriangle className="h-10 w-10 text-destructive" />
        <p className="text-sm text-muted-foreground">{error}</p>
        <Button variant="outline" onClick={() => window.location.reload()}>
          Try Again
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Certifications</h1>
          <p className="text-sm text-muted-foreground">
            Manage your ESG certifications and credentials
          </p>
        </div>

        <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
          <DialogTrigger
            render={
              <Button>
                <Plus className="mr-2 h-4 w-4" />
                Add Certification
              </Button>
            }
          />

          <DialogContent className="sm:max-w-md">
            <DialogHeader>
              <DialogTitle>Add Certification</DialogTitle>
              <DialogDescription>
                Submit a new certification for review. Once accepted, it will count
                toward your ESG score.
              </DialogDescription>
            </DialogHeader>

            <form
              onSubmit={form.handleSubmit(onSubmit)}
              className="space-y-4"
            >
              <div>
                <Label htmlFor="certType">Certification Type *</Label>
                <Controller
                  name="certificationTypeId"
                  control={form.control}
                  render={({ field }) => (
                    <Select
                      value={field.value || undefined}
                      onValueChange={field.onChange}
                    >
                      <SelectTrigger className="mt-1">
                        <SelectValue placeholder="Select type" />
                      </SelectTrigger>
                      <SelectContent>
                        {certTypes
                          .filter((ct) => ct.isActive)
                          .map((ct) => (
                            <SelectItem key={ct.id} value={ct.id}>
                              {ct.name}
                            </SelectItem>
                          ))}
                      </SelectContent>
                    </Select>
                  )}
                />
                {form.formState.errors.certificationTypeId && (
                  <p className="mt-1 text-xs text-destructive">
                    {form.formState.errors.certificationTypeId.message}
                  </p>
                )}
              </div>

              <div>
                <Label htmlFor="certificateNumber">Certificate Number</Label>
                <Input
                  id="certificateNumber"
                  {...form.register("certificateNumber")}
                  placeholder="Optional"
                  className="mt-1"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <Label htmlFor="issuedAt">Issued Date</Label>
                  <Input
                    id="issuedAt"
                    type="date"
                    {...form.register("issuedAt")}
                    className="mt-1"
                  />
                </div>
                <div>
                  <Label htmlFor="expiresAt">Expiry Date</Label>
                  <Input
                    id="expiresAt"
                    type="date"
                    {...form.register("expiresAt")}
                    className="mt-1"
                  />
                </div>
              </div>

              <div>
                <Label htmlFor="certFile">Certificate File (optional)</Label>
                <div className="mt-1">
                  <label
                    htmlFor="certFile"
                    className="flex cursor-pointer items-center gap-2 rounded-xl border-2 border-dashed p-4 text-sm text-muted-foreground transition-colors hover:border-brand-green hover:bg-brand-green-light"
                  >
                    <Upload className="h-5 w-5" />
                    {selectedFile ? selectedFile.name : "Click to upload PDF or image"}
                  </label>
                  <input
                    id="certFile"
                    type="file"
                    accept=".pdf,.jpg,.jpeg,.png"
                    className="hidden"
                    onChange={(e) => setSelectedFile(e.target.files?.[0] ?? null)}
                  />
                </div>
              </div>

              <DialogFooter>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => {
                    setDialogOpen(false);
                    form.reset();
                    setSelectedFile(null);
                  }}
                >
                  Cancel
                </Button>
                <Button type="submit" disabled={submitting}>
                  {submitting ? (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  ) : (
                    <ShieldCheck className="mr-2 h-4 w-4" />
                  )}
                  {submitting ? "Submitting..." : "Submit"}
                </Button>
              </DialogFooter>
            </form>
          </DialogContent>
        </Dialog>
      </div>

      {/* Certifications list */}
      {certs.length === 0 ? (
        <div className="flex flex-col items-center justify-center gap-4 rounded-2xl border bg-white py-16 shadow-sm">
          <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-muted/50">
            <ShieldCheck className="h-8 w-8 text-muted-foreground" />
          </div>
          <div className="text-center">
            <h3 className="text-base font-semibold text-foreground">
              No certifications yet
            </h3>
            <p className="mt-1 text-sm text-muted-foreground">
              Add your first certification to boost your ESG score and
              verification status.
            </p>
          </div>
          <Button onClick={() => setDialogOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Add Certification
          </Button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {certs.map((cert) => (
            <CertificationCard key={cert.id} cert={cert} />
          ))}
        </div>
      )}
    </div>
  );
}
