"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import { apiGetAuth } from "@/lib/api-client";
import type { SupplierDashboardStats } from "@/lib/types";
import { EsgBadge } from "@/components/suppliers/esg-badge";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Mail,
  MailPlus,
  ShieldCheck,
  Eye,
  EyeOff,
  ArrowRight,
  Upload,
  UserPen,
  AlertTriangle,
  TrendingUp,
} from "lucide-react";

function CompletenessRing({
  percent,
  size = 120,
}: {
  percent: number;
  size?: number;
}) {
  const strokeWidth = 8;
  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (percent / 100) * circumference;

  return (
    <div
      className="relative inline-flex items-center justify-center"
      style={{ width: size, height: size }}
      role="img"
      aria-label={`Profile ${percent}% complete`}
    >
      <svg width={size} height={size} className="-rotate-90" aria-hidden="true">
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="currentColor"
          strokeWidth={strokeWidth}
          className="text-muted/40"
        />
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="currentColor"
          strokeWidth={strokeWidth}
          strokeDasharray={circumference}
          strokeDashoffset={offset}
          strokeLinecap="round"
          className="text-brand-green transition-all duration-700 ease-out"
        />
      </svg>
      <div className="absolute flex flex-col items-center justify-center" aria-hidden="true">
        <span className="text-2xl font-bold text-foreground">{percent}%</span>
        <span className="text-xs text-muted-foreground">Complete</span>
      </div>
    </div>
  );
}

function StatCard({
  label,
  value,
  icon: Icon,
  accent = false,
}: {
  label: string;
  value: React.ReactNode;
  icon: React.ComponentType<{ className?: string }>;
  accent?: boolean;
}) {
  return (
    <div className="rounded-2xl border bg-white p-5 shadow-sm">
      <div className="flex items-start justify-between">
        <div>
          <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
            {label}
          </p>
          <div className="mt-2 text-2xl font-bold text-foreground">{value}</div>
        </div>
        <div
          aria-hidden="true"
          className={
            accent
              ? "flex h-10 w-10 items-center justify-center rounded-xl bg-brand-green/10"
              : "flex h-10 w-10 items-center justify-center rounded-xl bg-muted/50"
          }
        >
          <Icon className={accent ? "h-5 w-5 text-brand-green" : "h-5 w-5 text-muted-foreground"} />
        </div>
      </div>
    </div>
  );
}

function VerificationStatusBadge({ status }: { status: string }) {
  const lower = status.toLowerCase();
  const config: Record<string, { bg: string; text: string; label: string }> = {
    verified: { bg: "bg-green-100", text: "text-green-700", label: "Verified" },
    pending: { bg: "bg-yellow-100", text: "text-yellow-700", label: "Pending" },
    flagged: { bg: "bg-red-100", text: "text-red-700", label: "Flagged" },
    unverified: { bg: "bg-gray-100", text: "text-gray-600", label: "Unverified" },
  };
  const c = config[lower] ?? config.unverified;

  return (
    <span className={`inline-flex items-center rounded-full px-3 py-1 text-xs font-semibold ${c.bg} ${c.text}`}>
      {c.label}
    </span>
  );
}

function DashboardSkeleton() {
  return (
    <div role="status" aria-label="Loading dashboard" className="space-y-6">
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-48" />
      </div>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, i) => (
          <Skeleton key={i} className="h-28 rounded-2xl" />
        ))}
      </div>
      <div className="grid gap-6 lg:grid-cols-3">
        <Skeleton className="h-64 rounded-2xl lg:col-span-1" />
        <Skeleton className="h-64 rounded-2xl lg:col-span-2" />
      </div>
      <span className="sr-only">Loading dashboard data</span>
    </div>
  );
}

export default function SupplierDashboardPage() {
  const { token } = useAuth();
  const [stats, setStats] = useState<SupplierDashboardStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!token) return;

    async function fetchDashboard() {
      setLoading(true);
      setError(null);
      const res = await apiGetAuth<SupplierDashboardStats>(
        "/supplier/me/dashboard",
        token!
      );
      if (res.success && res.data) {
        setStats(res.data);
      } else {
        setError(res.error?.message ?? "Failed to load dashboard");
      }
      setLoading(false);
    }

    fetchDashboard();
  }, [token]);

  if (loading) {
    return <DashboardSkeleton />;
  }

  if (error || !stats) {
    return (
      <div role="alert" className="flex flex-col items-center justify-center gap-4 py-20">
        <AlertTriangle className="h-10 w-10 text-destructive" aria-hidden="true" />
        <p className="text-sm text-muted-foreground">{error ?? "Something went wrong"}</p>
        <Button variant="outline" onClick={() => window.location.reload()}>
          Try Again
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Page title + published status */}
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Dashboard</h1>
          <p className="text-sm text-muted-foreground">
            Welcome to your supplier portal
          </p>
        </div>
        {stats.isPublished ? (
          <div className="flex items-center gap-2 rounded-full border border-green-200 bg-green-50 px-4 py-2 text-sm font-medium text-green-700">
            <Eye className="h-4 w-4" />
            Profile Published
          </div>
        ) : (
          <div className="flex items-center gap-2 rounded-full border border-yellow-200 bg-yellow-50 px-4 py-2 text-sm font-medium text-yellow-700">
            <EyeOff className="h-4 w-4" />
            Profile Not Published
          </div>
        )}
      </div>

      {/* Stat cards */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard
          label="Total Leads"
          value={stats.totalLeads}
          icon={Mail}
          accent
        />
        <StatCard
          label="New Leads"
          value={stats.newLeads}
          icon={MailPlus}
          accent={stats.newLeads > 0}
        />
        <StatCard
          label="ESG Level"
          value={<EsgBadge level={stats.esgLevel} />}
          icon={TrendingUp}
        />
        <StatCard
          label="Verification"
          value={<VerificationStatusBadge status={stats.verificationStatus} />}
          icon={ShieldCheck}
        />
      </div>

      {/* Bottom row: completeness ring + quick actions */}
      <div className="grid gap-6 lg:grid-cols-3">
        {/* Profile completeness */}
        <div className="rounded-2xl border bg-white p-6 shadow-sm">
          <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
            Profile Completeness
          </h2>
          <div className="flex flex-col items-center gap-4">
            <CompletenessRing percent={stats.profileCompleteness} />
            {stats.profileCompleteness < 100 && (
              <p className="text-center text-xs text-muted-foreground">
                Complete your profile to improve your visibility and ESG score.
              </p>
            )}
            {stats.profileCompleteness >= 50 && !stats.isPublished && (
              <Link href="/dashboard/profile">
                <Button size="sm" className="mt-2">
                  Publish Profile
                  <ArrowRight className="ml-1 h-3 w-3" />
                </Button>
              </Link>
            )}
          </div>
        </div>

        {/* Quick actions + certification summary */}
        <div className="space-y-6 lg:col-span-2">
          {/* Quick actions */}
          <div className="rounded-2xl border bg-white p-6 shadow-sm">
            <h2 className="mb-4 text-sm font-semibold uppercase tracking-wide text-muted-foreground">
              Quick Actions
            </h2>
            <div className="grid gap-3 sm:grid-cols-3">
              <Link
                href="/dashboard/profile"
                className="flex items-center gap-3 rounded-xl border p-4 transition-colors hover:border-brand-green hover:bg-brand-green-light"
              >
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
                  <UserPen className="h-5 w-5 text-brand-green" />
                </div>
                <div>
                  <p className="text-sm font-medium">Complete Profile</p>
                  <p className="text-xs text-muted-foreground">
                    Edit your company info
                  </p>
                </div>
              </Link>

              <Link
                href="/dashboard/certifications"
                className="flex items-center gap-3 rounded-xl border p-4 transition-colors hover:border-brand-green hover:bg-brand-green-light"
              >
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
                  <Upload className="h-5 w-5 text-brand-green" />
                </div>
                <div>
                  <p className="text-sm font-medium">Upload Certification</p>
                  <p className="text-xs text-muted-foreground">
                    Add ESG credentials
                  </p>
                </div>
              </Link>

              <a
                href="/suppliers"
                target="_blank"
                rel="noopener noreferrer"
                className="flex items-center gap-3 rounded-xl border p-4 transition-colors hover:border-brand-green hover:bg-brand-green-light"
              >
                <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-brand-green/10">
                  <Eye className="h-5 w-5 text-brand-green" />
                </div>
                <div>
                  <p className="text-sm font-medium">View Profile</p>
                  <p className="text-xs text-muted-foreground">
                    See your public listing
                  </p>
                </div>
              </a>
            </div>
          </div>

          {/* Certifications summary */}
          <div className="rounded-2xl border bg-white p-6 shadow-sm">
            <div className="flex items-center justify-between">
              <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
                Certifications
              </h2>
              <Link
                href="/dashboard/certifications"
                className="text-xs font-medium text-brand-green hover:underline"
              >
                View All
              </Link>
            </div>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <div className="flex items-center gap-3 rounded-xl bg-green-50 p-4">
                <ShieldCheck className="h-5 w-5 text-green-600" />
                <div>
                  <p className="text-lg font-bold text-green-700">
                    {stats.activeCertifications}
                  </p>
                  <p className="text-xs text-green-600">Active</p>
                </div>
              </div>
              <div className="flex items-center gap-3 rounded-xl bg-yellow-50 p-4">
                <AlertTriangle className="h-5 w-5 text-yellow-600" />
                <div>
                  <p className="text-lg font-bold text-yellow-700">
                    {stats.expiringCertifications}
                  </p>
                  <p className="text-xs text-yellow-600">Expiring Soon</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
